using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FabGeo
{
    public static class Geo
    {
        /// <summary>
        /// Calculates latitude and longitude (in radians) from a point on a unit sphere
        /// </summary>
        /// <param name="pointOnUnitSphere"></param>
        /// <returns></returns>
        public static Coordinate PointToCoordinate(float3 pointOnUnitSphere)
        {
           //pointOnUnitSphere = math.normalize(pointOnUnitSphere);
            float latitude = math.atan2(pointOnUnitSphere.x, -pointOnUnitSphere.z);
            float longitude = math.asin(pointOnUnitSphere.y); 
            return new Coordinate(latitude, longitude);
        }

        /// <summary>
        /// Calculates a point on a unit sphere from latitude and longitude (in radians)
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static float3 CoordinateToPoint(Coordinate coordinate)
        {
            float y = math.sin(coordinate.latitude);
            float r = math.cos(coordinate.latitude);
            float x = math.sin(coordinate.longitude) * r;
            float z = -math.cos(coordinate.longitude) * r;
            return new float3(x, y, z);
        }

        /// <summary>
        /// Calculates a point on a unit sphere from a point on a cube
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static float3 PointOnCubeToPointOnSphere(float3 p)
        {
            float x2 = p.x * p.x;
            float y2 = p.y * p.y;
            float z2 = p.z * p.z;

            float x = p.x * math.sqrt(1f - (y2 + z2) / 2f + (y2 * z2) / 3f);
            float y = p.y * math.sqrt(1f - (z2 + x2) / 2f + (z2 * x2) / 3f);
            float z = p.z * math.sqrt(1f - (x2 + y2) / 2f + (x2 * y2) / 3f);

            return new float3(x, y, z);
        }
    }
}
