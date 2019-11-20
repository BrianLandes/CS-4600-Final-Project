
// Represents a side of the Hexagon, where zero is the east-most side and goes CCW.
using System;
using System.Collections.Generic;

public enum HexDirection {
	E, NE, NW, W, SW, SE
}

public static class HexDirectionUtils {

	public static int Int( this HexDirection direction ) {
		return (int)direction;
	}
	public static IEnumerable<HexDirection> All() {
		foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection))) {
			yield return direction;
		}
	}

	public static HexDirection Opposite (this HexDirection direction) {
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}

	public static HexDirection Last(this HexDirection direction) {
		return direction == HexDirection.E ? HexDirection.SE : (direction - 1);
	}

	public static HexDirection Next(this HexDirection direction) {
		return direction == HexDirection.SE ? HexDirection.E : (direction + 1);
	}

	public static HexDirection Last2(this HexDirection direction) {
		direction -= 2;
		return direction >= HexDirection.E ? direction : (direction + 6);
	}

	public static HexDirection Next2(this HexDirection direction) {
		direction += 2;
		return direction <= HexDirection.SE ? direction : (direction - 6);
	}

	/// <summary>
	/// Returns one of the corners that this direction touches.
	/// The corner returned is the most CW one relative to the hexagon.
	/// To get the second corner this side/direction touches simply use Next on the returned corner.
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static HexCorner GetCorner( this HexDirection direction ) {
		return (HexCorner)(int)direction;
	}
}