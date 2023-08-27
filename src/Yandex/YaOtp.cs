/* This file is taken from plugin KeeYaOtp v0.1.0
 * published here: https://github.com/norblik/KeeYaOtp
 * 
 * All credits go to the author
 *
 * Adjustments done here are solely done to
 * support creating a plgx plugin file and 
 * to use ProtectedString instead of string
 */
#region copyright
// KeeYaOtp, a KeePass plugin that generate one-time passwords for Yandex 2FA
// Copyright (C) 2020 norblik
//
// This plugin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This plugin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this plugin. If not, see <https://www.gnu.org/licenses/>.
//
// SPDX-License-Identifier: GPL-3.0-or-later
#endregion

using System;
using System.Security.Cryptography;

namespace KeePassOTP
{
  public delegate DateTime UtcNowDelegate();

  public class YaOtp
  {
    private readonly UtcNowDelegate _utcNow;
    private readonly byte[] keyHash;

    public YaOtp(YandexSecret secret, YandexPin pin, UtcNowDelegate utcNow)
    {
      if (secret == null) throw new ArgumentNullException("secret");
      if (pin == null) throw new ArgumentNullException("pin");
      if (pin.Length != secret.PinLength) throw new ArgumentException("Pin length mismatch");
      if (utcNow == null) throw new ArgumentNullException("utcNow");
      _utcNow = utcNow;

      var p = pin.Data;
      var s = secret.Data;
      var key = new byte[p.Length + s.Length];
      Array.Copy(p, 0, key, 0, p.Length);
      Array.Copy(s, 0, key, p.Length, s.Length);

      using (SHA256 sha256 = new SHA256Managed())
      {
        keyHash = sha256.ComputeHash(key);
      }
      if (keyHash[0] == 0)
      {
        var temp = new byte[keyHash.Length - 1];
        Array.Copy(keyHash, 1, temp, 0, temp.Length);
        keyHash = temp;
      }
    }

    static private readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    static private byte[] ToBigEndianArray(long value)
    {
      var res = new byte[8];

      for (int i = 7; i >= 0; i--)
      {
        res[i] = (byte)(value & 0xFF);
        value >>= 8;
      }

      return res;
    }

    static private long FromBigEndianArray(int startIndex, byte[] array)
    {
      long value = 0;
      for (int i = startIndex; i <= startIndex + 7; i++)
        value = value << 8 | array[i];
      return value;
    }

    static private string ToBase26(long value, byte length)
    {
      var chars = new char[length];
      var index = length - 1;
      while (index >= 0)
      {
        chars[index] = (char)('a' + value % 26); // 'a'... 'z'
        value /= 26;
        index--;
      }

      return new string(chars);
    }

    private const long timePeriodSec = 30;
    private const byte otpLength = 8;

    private long GetUtcSecNow()
    {
      return (_utcNow().Ticks - unixEpoch.Ticks) / 10000 / 1000;
    }

    public string ComputeOtp()
    {
      var curPeriod = GetUtcSecNow() / timePeriodSec;

      byte[] periodHash = null;
      using (HMACSHA256 hmac = new HMACSHA256(keyHash))
      {
        periodHash = hmac.ComputeHash(ToBigEndianArray(curPeriod));
      }
      var periodHashSlice = FromBigEndianArray(periodHash[periodHash.Length - 1] & 15, periodHash) & long.MaxValue;

      long limitValue = (long)Math.Pow(26.0d, otpLength); // 26 symbols in eng alphabet
      return ToBase26(periodHashSlice % limitValue, otpLength);
    }

    public byte GetRemainingSeconds()
    {
      var secNow = GetUtcSecNow();
      var periodNow = secNow / timePeriodSec;
      var secNextPeriod = (periodNow + 1) * timePeriodSec;
      return (byte)(secNextPeriod - secNow);
    }
  }
}
