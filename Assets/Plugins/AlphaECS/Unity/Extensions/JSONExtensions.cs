using UnityEngine;
using SimpleJSON;

/*
 * http://answers.unity3d.com/questions/1322769/parsing-nested-arrays-with-jsonutility.html
 */

public static class JSONExtensions
{
	public static Vector2 AsVector2 (this JSONNode node)
	{
		var values = JSON.Parse(node);
		return new Vector2(values["x"].AsFloat, values["y"].AsFloat);
	}

	public static Vector2 AsVector2 (this JSONNode node, Vector2 value)
	{
		var jsonObject = node.AsObject;
		jsonObject.Add("x", new JSONNumber(value.x));
		jsonObject.Add("y", new JSONNumber(value.y));
		return jsonObject.AsVector2 ();
	}



	public static Vector3 AsVector3 (this JSONNode node)
	{
		var values = JSON.Parse(node);
		return new Vector3(values["x"].AsFloat, values["y"].AsFloat, values["z"].AsFloat);
	}

	public static Vector3 AsVector3(this JSONNode node, Vector3 value)
	{
		var jsonObject = node.AsObject;
		jsonObject.Add("x", new JSONNumber(value.x));
		jsonObject.Add("y", new JSONNumber(value.y));
		jsonObject.Add("z", new JSONNumber(value.z));
		return jsonObject.AsVector3 ();
	}

	public static Vector4 AsVector4 (this JSONNode node)
	{
		var values = JSON.Parse(node);
		return new Vector4(values["x"].AsFloat, values["y"].AsFloat, values["z"].AsFloat, values["w"].AsFloat);
	}

	public static Vector4 AsVector4 (this JSONNode node, Vector4 value)
	{
		var jsonObject = node.AsObject;
		jsonObject.Add("x", new JSONNumber(value.x));
		jsonObject.Add("y", new JSONNumber(value.y));
		jsonObject.Add("z", new JSONNumber(value.z));
		jsonObject.Add("w", new JSONNumber(value.w));
		return jsonObject.AsVector4 ();
	}

	public static Color AsColor (this JSONNode node)
	{
		var jsonObject = node.AsObject;
		return new Color(jsonObject["r"].AsFloat, jsonObject["g"].AsFloat, jsonObject["b"].AsFloat, jsonObject["a"].AsFloat);
	}

	public static Color AsColor (this JSONNode node, Color value)
	{
		var jsonObject = node.AsObject;
		jsonObject.Add("r", new JSONNumber(value.r));
		jsonObject.Add("g", new JSONNumber(value.g));
		jsonObject.Add("b", new JSONNumber(value.b));
		jsonObject.Add("a", new JSONNumber(value.a));
		return jsonObject.AsColor ();
	}

	public static JSONObject AsJSONObject(this Vector2 value)
	{
		var jsonObject = new JSONObject();
		jsonObject.Add ("x", value.x);
		jsonObject.Add ("y", value.y);
		return jsonObject;
	}

	public static JSONObject AsJSONObject(this Vector3 value)
	{
		var jsonObject = new JSONObject();
		jsonObject.Add ("x", value.x);
		jsonObject.Add ("y", value.y);
		jsonObject.Add ("z", value.z);
		return jsonObject;
	}

	public static JSONObject AsJSONObject(this Vector4 value)
	{
		var jsonObject = new JSONObject();
		jsonObject.Add ("x", value.x);
		jsonObject.Add ("y", value.y);
		jsonObject.Add ("z", value.z);
		jsonObject.Add ("w", value.w);
		return jsonObject;
	}

	public static JSONObject AsJSONObject(this Color value)
	{
		var jsonObject = new JSONObject();
		jsonObject.Add ("r", value.r);
		jsonObject.Add ("g", value.g);
		jsonObject.Add ("b", value.b);
		jsonObject.Add ("a", value.a);
		return jsonObject;
	}
}