using System.Collections.Generic;
using UnityEngine;

namespace BPC.Utils
{
    public static class HexGridUtils
    {
        /// <summary>
        /// Takes what Vector2.SignedAngle returns
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector2Int AngleToHexGridNeighborGridPos(float angle)
        {
            return HexGridNeighbors[AngleToHexGridNeighbor(angle)];
        }

        public enum HexGridNeighbor
        {
            East = 0,
            NorthEast = 1,
            NorthWest = 2,
            West = 3,
            SouthWest = 4,
            SouthEast = 5
        }

        public static Vector3 HexGridToWorld(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * 0.5f,
                -gridPos.y * 0.88f);
        }

        private static HexGridNeighbor AngleToHexGridNeighbor(float angle)
        {
            if (angle > 0 && angle <= 30)
            {
                return HexGridNeighbor.East;
            }

            if (angle > 30 && angle <= 90)
            {
                return HexGridNeighbor.NorthEast;
            }

            if (angle > 90 && angle < 150)
            {
                return HexGridNeighbor.NorthWest;
            }

            if (angle > -180 && angle <= -90)
            {
                return HexGridNeighbor.SouthWest;
            }

            if (angle > -90 && angle <= 0)
            {
                return HexGridNeighbor.SouthEast;
            }

            return HexGridNeighbor.West;
        }

        private static readonly Dictionary<HexGridNeighbor, Vector2Int> HexGridNeighbors =
            new Dictionary<HexGridNeighbor, Vector2Int>
            {
                {HexGridNeighbor.East, new Vector2Int(2, 0)},
                {HexGridNeighbor.NorthEast, new Vector2Int(1, -1)},
                {HexGridNeighbor.NorthWest, new Vector2Int(-1, -1)},
                {HexGridNeighbor.West, new Vector2Int(-2, 0)},
                {HexGridNeighbor.SouthWest, new Vector2Int(-1, 1)},
                {HexGridNeighbor.SouthEast, new Vector2Int(1, 1)}
            };

        public static List<Vector2Int> GetAllNeighborGridPositions(Vector2Int gridPos)
        {
            var neighbors = new List<Vector2Int>(6);
            foreach (var neighbor in HexGridNeighbors)
            {
                neighbors.Add(gridPos + neighbor.Value);
            }

            return neighbors;
        }
    }
}