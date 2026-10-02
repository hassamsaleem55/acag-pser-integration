using ACAG_PSER_Integration.Services.Enums;
using ACAG_PSER_Integration.Services.Interfaces;

namespace ACAG_PSER_Integration.Services;

public sealed class CredentialFormatter : ICredentialFormatter
{
    private const string SpecialChars = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

    public string Format(string value, CredentialType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return Transform(value, type, isFormat: true);
    }

    public string ExtractOriginalValue(string value, CredentialType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Transform(value, type, isFormat: false);
    }

    private static string Transform(string value, CredentialType type, bool isFormat)
    {
        int length = value.Length;

        return string.Create(length, (value, type, isFormat), static (span, state) =>
        {
            var (source, credType, format) = state;
            int totalLength = source.Length;

            for (int i = 0; i < totalLength; i++)
            {
                int step;
                bool isForwardInFormat;

                if (credType == CredentialType.Password)
                {
                    // Password rule:
                    // Steps descend from totalLength down to 1.
                    // 1st char (i=0) jumps backward, 2nd (i=1) jumps forward, etc.
                    step = totalLength - i;
                    isForwardInFormat = (i % 2 != 0);
                }
                else
                {
                    // Username rule:
                    // Steps ascend from 1 up to totalLength.
                    // 1st char (i=0) jumps forward, 2nd (i=1) jumps backward, etc.
                    step = i + 1;
                    isForwardInFormat = (i % 2 == 0);
                }

                // Reverse the direction when extracting
                bool jumpAhead = format ? isForwardInFormat : !isForwardInFormat;
                int direction = jumpAhead ? 1 : -1;
                int shift = direction * step;

                span[i] = ShiftCharacter(source[i], shift);
            }
        });
    }

    private static char ShiftCharacter(char c, int shift)
    {
        if (c is >= 'a' and <= 'z')
        {
            return Shift(c, 'a', 26, shift);
        }

        if (c is >= 'A' and <= 'Z')
        {
            return Shift(c, 'A', 26, shift);
        }

        if (c is >= '0' and <= '9')
        {
            return Shift(c, '0', 10, shift);
        }

        int specialIndex = SpecialChars.IndexOf(c);
        if (specialIndex >= 0)
        {
            return ShiftFromSet(SpecialChars, specialIndex, shift);
        }

        return c;
    }

    private static char Shift(char c, char baseChar, int alphabetSize, int shift)
    {
        int offset = (c - baseChar + (shift % alphabetSize)) % alphabetSize;
        if (offset < 0)
        {
            offset += alphabetSize;
        }

        return (char)(baseChar + offset);
    }

    private static char ShiftFromSet(string charSet, int currentIndex, int shift)
    {
        int size = charSet.Length;
        int offset = (currentIndex + (shift % size)) % size;
        if (offset < 0)
        {
            offset += size;
        }

        return charSet[offset];
    }
}