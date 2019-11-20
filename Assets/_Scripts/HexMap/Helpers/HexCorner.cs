using System;
using System.Collections.Generic;
/// <summary>
/// corner zero is 4-o'clock/SE, moves CCW.
/// Why? Why not.
/// </summary>
public enum HexCorner {
	SE, NE, N, NW, SW, S,

}

public static class HexCornerUtils {
	public static IEnumerable<HexCorner> AllCorners() {
		foreach (HexCorner corner in Enum.GetValues(typeof(HexCorner))) {
			yield return corner;
		}
	}

	/// <summary>
	/// Returns the next corner CCW
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static HexCorner Next(this HexCorner corner) {
		return corner == HexCorner.S ? HexCorner.SE : (corner + 1);
	}

	/// <summary>
	/// Returns the last corner CW
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static HexCorner Last(this HexCorner corner) {
		return corner == HexCorner.SE ? HexCorner.S : (corner - 1);
	}

	public static HexCorner Opposite(this HexCorner corner) {
		return (int)corner < 3 ? (corner + 3) : (corner - 3);
	}

	public static int GetInt(this HexCorner corner) {
		return (int)corner;
	}

	/// <summary>
	/// Returns one of the sides that this corner touches.
	/// The side returned is the most CCW one relative to the hexagon.
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static HexDirection GetDirection(this HexCorner corner) {
		return (HexDirection)(int)corner;
	}
}
