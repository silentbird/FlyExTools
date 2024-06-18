
using System.Text;

namespace pure.refactor.serialize
{
  public static class StringConvert
  {
    private static Encoding codePage = Encoding.UTF8;

    public static byte[] GetByte(string temp)
    {
      return temp != null ? StringConvert.codePage.GetBytes(temp) : (byte[]) null;
    }

    public static string GetString(byte[] buffer, int index, int len)
    {
      return StringConvert.codePage.GetString(buffer, index, len);
    }

    public static unsafe string GetString(byte* buf, int len)
    {
      return StringConvert.codePage.GetString(buf, len);
    }
  }
}
