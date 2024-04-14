using System;
using UnityEngine;

namespace MathHelper
{
	/// <summary>
	/// ベジェ曲線を扱うためのヘルパークラス
	/// </summary>
	public static class BezierCurve
	{
		public static Vector3 GetPoint(float t, params Vector3[] points)
		{
			Vector3 result = Vector3.zero;

			var reverseT = 1f - t;
			var pLength = points.Length - 1;

			for (var i = 0; i < points.Length; i++)
			{
				var point = points[i];
				float rPow = Mathf.Pow(reverseT, pLength - i);
				float tPow = Mathf.Pow(t, i);
				if (i == 0 || i == pLength)
				{
					result += rPow * tPow * point;
				}
				else
				{
					result += pLength * rPow * tPow * point;
				}
			}

			return result;
		}
		/// <summary>
		/// とりあえず正確な距離ではなく近似値で求める
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>

		public static float GetApproximateLength(params Vector3[] points)
		{
			if (points.Length <= 1)
			{
				return 0f;
			}
			var beginPoint = points[0];
			var endPoint = points[points.Length - 1];
			if (points.Length == 2)
			{
				return Vector3.Distance(beginPoint, endPoint);
			}
			var currentPoint = beginPoint;
			int samplingNum = 200;
			float distance = 0f;
			for (int i = 1; i < samplingNum - 1; i++)
			{
				var point = GetPoint(i / (float)samplingNum, points);
				distance += Vector3.Distance(currentPoint, point);
				currentPoint = point;
			}
			distance += Vector3.Distance(endPoint, currentPoint);
			return distance;
		}

		/// <summary>
		/// とりあえず正確な距離ではなく近似値で求める
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>

		public static Vector3 GetApproximateDistancePoint(float distance, params Vector3[] points)
		{
			if (points.Length < 1)
			{
				return Vector3.zero;
			}
			else if (points.Length == 1)
			{
				return points[0];
			}
			var beginPoint = points[0];
			var endPoint = points[points.Length - 1];
			var currentPoint = beginPoint;
			int samplingNum = 200;
			float currentDistance = 0f;
			for (int i = 1; i < samplingNum - 1; i++)
			{
				var ratio = i / (float)samplingNum;
				var point = GetPoint(ratio, points);
				var addDistance = Vector3.Distance(currentPoint, point);
				if (currentDistance <= distance && distance < currentDistance + addDistance)
				{
					var addRatio = (currentDistance - distance) / addDistance;
					return GetPoint(ratio + (addRatio * (1 / samplingNum)), points);
				}
				currentDistance += addDistance;
				currentPoint = point;
			}
			return endPoint;
		}

		//public static float GetApproximateLength2(params Vector3[] points)
		//{
		//	if (points.Length <= 1)
		//	{
		//		return 0f;
		//	}
		//	var beginPoint = points[0];
		//	var endPoint = points[points.Length - 1];
		//	if (points.Length == 2)
		//	{
		//		return Vector3.Distance(beginPoint, endPoint);
		//	}
		//	var pathDistance = Vector3.Distance(beginPoint, endPoint);
		//	var controlDistance = 0f;
		//	for (var i = 0; i < points.Length - 1; i++)
		//	{
		//		controlDistance += Vector3.Distance(points[i], points[i + 1]);
		//	}
		//	return (pathDistance + controlDistance) * 0.5f;
		//}

	}
}
