using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;

/*public class FixedStringExtensions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/
namespace Core.Extensions
{
	public struct FixedStringExtensions
	{
		public static FixedString64 NULL64 => $"";
		public static FixedString32 NULL32 => $"";
		public static FixedString128 NULL128 => $"";
		public static FixedString512 NULL512 => $"";
		public static FixedString4096 NULL4096 => $"";
	}
}
