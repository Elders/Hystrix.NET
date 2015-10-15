using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Java.Util.Concurrent.Helpers
{
	/// <summary>
	/// Collection of static methods that aid in serialization.
	/// </summary>
	public static class SerializationUtilities //NET_ONLY
	{
		/// <summary>
		/// Writes the serializable fields to the SerializationInfo object, which stores all the data needed to serialize the specified object object.
		/// </summary>
		/// <param name="info">SerializationInfo parameter from the GetObjectData method.</param>
		/// <param name="context">StreamingContext parameter from the GetObjectData method.</param>
		/// <param name="instance">Object to serialize.</param>
		public static void DefaultWriteObject( SerializationInfo info, StreamingContext context, Object instance )
		{
			Type thisType = instance.GetType();
			MemberInfo[] mi = FormatterServices.GetSerializableMembers( thisType, context );
			for ( int i = 0; i < mi.Length; i++ )
			{
				info.AddValue( mi[i].Name, ( (FieldInfo) mi[i] ).GetValue( instance ) );
			}
		}

		/// <summary>
		/// Reads the serialized fields written by the DefaultWriteObject method.
		/// </summary>
		/// <param name="info">SerializationInfo parameter from the special deserialization constructor.</param>
		/// <param name="context">StreamingContext parameter from the special deserialization constructor</param>
		/// <param name="instance">Object to deserialize.</param>
		public static void DefaultReadObject( SerializationInfo info, StreamingContext context, Object instance )
		{
			Type thisType = instance.GetType();
			MemberInfo[] mi = FormatterServices.GetSerializableMembers( thisType, context );
			for ( int i = 0; i < mi.Length; i++ )
			{
				FieldInfo fi = (FieldInfo) mi[i];
				fi.SetValue( instance, info.GetValue( fi.Name, fi.FieldType ) );
			}
		}
	}
}