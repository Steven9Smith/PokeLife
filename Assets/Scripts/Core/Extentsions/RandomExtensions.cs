using System;

namespace Core.Extensions{
	public struct RandomExtensions {
		public static int GenerateRandomInt()
		{
			DateTime now = DateTime.Now;
			string number = now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + now.Millisecond.ToString();
			return int.Parse(number);
		}
	}
}