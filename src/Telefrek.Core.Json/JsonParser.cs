using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Parses JsonElements from a string or collection of bytes
    /// </summary>
    public class JsonParser
    {
        // Characters that help inform Json syntax
        private const string JSON_OBJECT_TERMINATORS = ",}"; // Object property terminators
        private const string JSON_ARRAY_TERMINATORS = ",]"; // Array item terminators
        private const string JSON_NON_STRING = "-0123456789nbt"; // Non-String primitive identifiers

        const byte QUOTE = (byte)'\"';
        const byte COLON = (byte)':';
        const byte COMMA = (byte)',';
        const byte ESCAPE = (byte)'\\';
        const byte START_OBJECT = (byte)'{';
        const byte END_OBJECT = (byte)'}';
        const byte START_ARRAY = (byte)'[';
        const byte END_ARRAY = (byte)']';

        const byte SPACE = (byte)' ';
        const byte TAB = (byte)'\t';
        const byte RETURN = (byte)'\r';
        const byte NEWLINE = (byte)'\n';

        private static readonly byte[] JSON_TOKENS = new byte[] { QUOTE, COLON, COMMA, ESCAPE, START_ARRAY, START_OBJECT, END_ARRAY, END_OBJECT };

        // Flag for controlling buffer size
        private const int BUFFER_SIZE = 4096;

        /// <summary>
        /// Parse the stream as Json asynchronously
        /// 
        /// Note: The stream is not closed by this operation
        /// </summary>
        /// <param name="stream">The stream to parse</param>
        /// <returns>A JsonElement that represents the stream</returns>
        public static async Task<JsonElement> ParseAsync(Stream stream) => await ParseAsync(stream, CancellationToken.None).ConfigureAwait(false);

        /// <summary>
        /// Parse the sequence as Json
        /// </summary>
        /// <param name="stream">The sequence to parse</param>
        /// <returns>A JsonElement that represents the sequence</returns>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the buffer is not proper Json</exception>
        internal static JsonElement ParseBuffer(ReadOnlySequence<byte> buffer)
        {
            var tree = new Stack<JsonElement>();
            var current = (JsonElement)null;
            var parent = (JsonElement)null;
            var isName = false;

            while (true)
            {
                if (buffer.Length > 0)
                {
                    Trim(ref buffer);
                    switch (buffer.FirstSpan[0])
                    {
                        case START_ARRAY:
                            tree.Push(current);
                            current = new JsonArray();
                            buffer = buffer.Slice(1);
                            break;
                        case END_ARRAY:
                            parent = tree.Pop();
                            buffer = buffer.Slice(1);
                            if (parent != null)
                            {
                                if (parent.IsJsonArray())
                                    parent.AsJsonArray().Items.Add(current);
                                else if (parent.IsJsonObject())
                                    parent.AsJsonObject().Properties[parent.AsJsonObject().Properties.Count - 1].Value = current;

                                current = parent;
                            }
                            break;
                        case START_OBJECT:
                            tree.Push(current);
                            isName = true;
                            current = new JsonObject();
                            buffer = buffer.Slice(1);
                            break;
                        case END_OBJECT:
                            parent = tree.Pop();
                            isName = false;
                            buffer = buffer.Slice(1);
                            if (parent != null)
                            {
                                if (parent.IsJsonArray())
                                    parent.AsJsonArray().Items.Add(current);
                                else if (parent.IsJsonObject())
                                {
                                    parent.AsJsonObject().Properties[parent.AsJsonObject().Properties.Count - 1].Value = current;
                                    isName = true;
                                }
                                current = parent;
                            }
                            break;
                        case COMMA:
                            // End of one object, properties should add here? child objects in array?
                            buffer = buffer.Slice(1);
                            isName = current != null && current.IsJsonObject();
                            break;
                        case COLON:
                            buffer = buffer.Slice(1);
                            isName = false;
                            break;
                        default:
                            // Check the current state for property reading
                            if (isName && TryReadString(ref buffer, out ReadOnlySequence<byte> name))
                            {
                                // Verify the state
                                if (current == null || !current.IsJsonObject())
                                    throw new InvalidJsonFormatException("Attempted to read property without parent object");

                                // Create the property
                                var prop = new JsonProperty { Name = Encoding.UTF8.GetString(name.ToArray()), Value = JsonNull.Instance };
                                current.AsJsonObject().Properties.Add(prop);
                                isName = false;
                            }
                            // Try to read a primitive off the buffer
                            else if (TryReadPrimitive(ref buffer, out JsonElement element))
                            {
                                if (current == null)
                                {
                                    current = element;
                                }
                                else if (current.IsJsonArray())
                                {
                                    current.AsJsonArray().Items.Add(element);
                                }
                                else if (current.IsJsonObject())
                                {
                                    var prop = current.AsJsonObject().Properties[current.AsJsonObject().Properties.Count - 1];
                                    prop.Value = element;
                                }
                                else throw new InvalidJsonFormatException("Attempted to add primitive to non object/array");
                            }
                            // This is an invalid buffer
                            else throw new InvalidJsonFormatException("Failed to parse the buffer");
                            break;
                    }
                }

                // Check for completion
                if (buffer.Length == 0) break;
            }

            return current;
        }

        /// <summary>
        /// Parse the stream as Json asynchronously
        /// </summary>
        /// <param name="stream">The stream to parse</param>
        /// <param name="token">The cancellation token to check for timeouts</param>
        /// <param name="leaveOpen">Flag to control stream state after processing</param>
        /// <returns>A JsonElement that represents the stream</returns>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the stream is not proper Json</exception>
        internal static async Task<JsonElement> ParseAsync(Stream stream, CancellationToken token, bool leaveOpen = true)
        {
            var reader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: leaveOpen));
            var tree = new Stack<JsonElement>();
            var current = (JsonElement)null;
            var parent = (JsonElement)null;
            var isName = false;

            while (true)
            {
                // Handle cancellation
                token.ThrowIfCancellationRequested();

                // Read another chunk
                var result = await reader.ReadAsync(token).ConfigureAwait(false);
                var buffer = result.Buffer;

                try
                {
                    if (buffer.Length > 0)
                    {
                        Trim(ref buffer);
                        switch (buffer.FirstSpan[0])
                        {
                            case START_ARRAY:
                                tree.Push(current);
                                current = new JsonArray();
                                buffer = buffer.Slice(1);
                                break;
                            case END_ARRAY:
                                parent = tree.Pop();
                                buffer = buffer.Slice(1);
                                if (parent != null)
                                {
                                    if (parent.IsJsonArray())
                                        parent.AsJsonArray().Items.Add(current);
                                    else if (parent.IsJsonObject())
                                        parent.AsJsonObject().Properties[parent.AsJsonObject().Properties.Count - 1].Value = current;

                                    current = parent;
                                }
                                break;
                            case START_OBJECT:
                                tree.Push(current);
                                isName = true;
                                current = new JsonObject();
                                buffer = buffer.Slice(1);
                                break;
                            case END_OBJECT:
                                parent = tree.Pop();
                                isName = false;
                                buffer = buffer.Slice(1);
                                if (parent != null)
                                {
                                    if (parent.IsJsonArray())
                                        parent.AsJsonArray().Items.Add(current);
                                    else if (parent.IsJsonObject())
                                    {
                                        parent.AsJsonObject().Properties[parent.AsJsonObject().Properties.Count - 1].Value = current;
                                        isName = true;
                                    }
                                    current = parent;
                                }
                                break;
                            case COMMA:
                                // End of one object, properties should add here? child objects in array?
                                buffer = buffer.Slice(1);
                                isName = current != null && current.IsJsonObject();
                                break;
                            case COLON:
                                buffer = buffer.Slice(1);
                                isName = false;
                                break;
                            default:
                                // Check the current state for property reading
                                if (isName && TryReadString(ref buffer, out ReadOnlySequence<byte> name))
                                {
                                    // Verify the state
                                    if (current == null || !current.IsJsonObject())
                                        throw new InvalidJsonFormatException("Attempted to read property without parent object");

                                    // Add the property
                                    var prop = new JsonProperty { Name = Encoding.UTF8.GetString(name.ToArray()), Value = JsonNull.Instance };
                                    current.AsJsonObject().Properties.Add(prop);
                                    isName = false;
                                }
                                // Try to read a primitive
                                else if (TryReadPrimitive(ref buffer, out JsonElement element))
                                {
                                    if (current == null)
                                    {
                                        current = element;
                                    }
                                    else if (current.IsJsonArray())
                                    {
                                        current.AsJsonArray().Items.Add(element);
                                    }
                                    else if (current.IsJsonObject())
                                    {
                                        var prop = current.AsJsonObject().Properties[current.AsJsonObject().Properties.Count - 1];
                                        prop.Value = element;
                                    }
                                    else throw new InvalidJsonFormatException("Attempted to add primitive to non object/array");
                                }
                                // Fail if there are no more bytes available in the stream
                                else if (result.IsCompleted) throw new InvalidJsonFormatException("Stream does not contain well formed Json");
                                break;
                        }
                    }
                }
                finally
                {
                    // Advance thr reader
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }

                // Check for completion
                if (result.IsCompleted && buffer.Length == 0) break;
            }

            return current;
        }

        /// <summary>
        /// Attempts to read a number from the current buffer
        /// </summary>
        /// <param name="current">The current buffer sequence</param>
        /// <param name="value">The location to store the successful read</param>
        /// <returns>True if value was populated with a number</returns>
        static bool TryReadNumber(ref ReadOnlySequence<byte> current, out ReadOnlySequence<byte> value)
        {
            value = default;
            Trim(ref current);
            var pos = current.Start;


            if (current.TryGet(ref pos, out ReadOnlyMemory<byte> buffer, true))
            {
                var len = buffer.Length;
                var offset = -1;
                var span = buffer.Span;
                for (var i = 0; i < len; ++i)
                {
                    if (span[i].IsNumeric()) continue;
                    offset = i;
                    break;
                }

                if (offset > 0)
                {
                    value = current.Slice(pos, offset);
                    current = current.Slice(offset);
                    return true;
                }
                else if (span[0].IsNumeric())
                {
                    value = current.Slice(pos);
                    current = current.Slice(current.End);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trims whitespace from the current buffer
        /// </summary>
        /// <param name="current">The current sequence</param>
        static void Trim(ref ReadOnlySequence<byte> current)
        {
            var total = 0;

            foreach (var segment in current)
            {
                for (var i = 0; i < segment.Length; ++i, ++total)
                {
                    switch (segment.Span[i])
                    {
                        case TAB:
                        case SPACE:
                        case RETURN:
                        case NEWLINE:
                            break;
                        default:
                            current = current.Slice(total);
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to read a string from the current buffer
        /// </summary>
        /// <param name="current">The current buffer sequence</param>
        /// <param name="value">The location to store the successful read</param>
        /// <returns>True if value was populated with a string</returns>
        static bool TryReadString(ref ReadOnlySequence<byte> current, out ReadOnlySequence<byte> token)
        {
            token = default;

            var escaped = false;
            var total = 0;
            var start = -1;

            foreach (var segment in current)
            {
                var span = segment.Span;
                for (var i = 0; i < span.Length; ++i, total++)
                {
                    if (escaped) // Skip the next byte
                    {
                        escaped = false;
                        continue;
                    }
                    else if (span[i] == ESCAPE)
                    {
                        escaped = true;
                        continue;
                    }

                    if (span[i] == QUOTE)
                    {
                        if (start >= 0)
                        {
                            // Need to remove any escapes
                            token = current.Slice(start + 1, total - 1);
                            current = current.Slice(current.GetPosition(total + 1));
                            return true;
                        }
                        else
                        {
                            start = total;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to read a primitive from the current buffer
        /// </summary>
        /// <param name="current">The current buffer sequence</param>
        /// <param name="value">The location to store the successful read</param>
        /// <returns>True if value was populated with a primitive</returns>
        static bool TryReadPrimitive(ref ReadOnlySequence<byte> current, out JsonElement element)
        {
            element = null;

            // Trim the results
            Trim(ref current);

            // Check the next byte
            if (current.Length > 0)
            {
                if (current.FirstSpan[0].IsNumeric())
                {
                    // Number
                    if (TryReadNumber(ref current, out ReadOnlySequence<byte> token))
                    {
                        var n = Double.Parse(Encoding.UTF8.GetString(token.ToArray()));
                        element = Math.Abs(n % 1) <= (Double.Epsilon * 100) ? new JsonNumber { Value = (long)n } : (JsonElement)n;
                        return true;
                    }
                }
                else if (current.FirstSpan[0] == QUOTE)
                {
                    // String
                    if (TryReadString(ref current, out ReadOnlySequence<byte> token))
                    {
                        element = Encoding.UTF8.GetString(token.ToArray());
                        return true;
                    }
                }
                else if (current.FirstSpan[0] == (byte)'n')
                {
                    // Null
                    if (current.Length > 3)
                    {
                        var pos = current.Start;
                        if (current.TryGet(ref pos, out ReadOnlyMemory<byte> test))
                            if (test.Span[1] == (byte)'u' && test.Span[2] == (byte)'l' && test.Span[3] == (byte)'l')
                            {
                                current = current.Slice(4);
                                element = JsonNull.Instance;
                                return true;
                            }
                    }
                }
                else if (current.FirstSpan[0] == (byte)'t')
                {
                    // Boolean
                    if (current.Length > 3)
                    {
                        var pos = current.Start;
                        if (current.TryGet(ref pos, out ReadOnlyMemory<byte> test))
                            if (test.Span[1] == (byte)'r' && test.Span[2] == (byte)'u' && test.Span[3] == (byte)'e')
                            {
                                current = current.Slice(4);
                                element = true;
                                return true;
                            }
                    }
                }
                else if (current.FirstSpan[0] == (byte)'f')
                {
                    // Boolean
                    if (current.Length > 4)
                    {
                        var pos = current.Start;
                        if (current.TryGet(ref pos, out ReadOnlyMemory<byte> test))
                            if (test.Span[1] == (byte)'a' && test.Span[2] == (byte)'l' && test.Span[3] == (byte)'s' && test.Span[4] == 'e')
                            {
                                current = current.Slice(5);
                                element = false;
                                return true;
                            }
                    }

                }
            }

            return false;
        }
    }
}