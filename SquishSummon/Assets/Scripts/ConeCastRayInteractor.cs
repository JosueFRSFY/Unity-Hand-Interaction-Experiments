/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2022.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Preview.HandRays
{
    /// <summary>
    /// Performs a conecast in the handraydirection passed in.
    /// Returns primary hit as the object closest to a straight hand ray
    /// </summary>
    public class ConeCastRayInteractor : HandRayInteractor
    {
        /// <summary>
        /// Max distance to conecast in 
        /// </summary>
        public float maxRayDistance = 50f;
        /// <summary>
        /// The max radius used in the spherecast before objects are filtered out by the conecast angle
        /// </summary>
        public float conecastMaxRadius = 1f;

        /// <summary>
        /// The angle of the cone used to filter out objects from a spherecast into a coneshape
        /// </summary>
        public float conecastAngle = 30f;

        protected override int UpdateRayInteractorLogic(HandRayDirection handRayDirection, out RaycastHit[] results, out RaycastHit primaryHit)
        {
            int hits = ConeCastExtension.ConeCastAll(handRayDirection.RayOrigin, conecastMaxRadius, handRayDirection.Direction, maxRayDistance, conecastAngle, layerMask, out results);
            Vector3 endPos = Vector3.zero;

            if (hits > 0)
            {
                primaryHit = results[0];
                float currentShortestDistance = Mathf.Infinity;
                Ray ray = new Ray(handRayDirection.AimPosition, handRayDirection.Direction);

                for (int i = 0; i < hits; i++)
                {
                    //Prioritise other colliders over the floor collider
                    if (results[i].collider.gameObject.layer != farFieldLayerManager.FloorLayer || endPos == Vector3.zero)
                    {
                        Vector3 point = results[i].collider.transform.position;
                        float newDistance = Vector3.Cross(ray.direction, point - ray.origin).magnitude;

                        if (newDistance < currentShortestDistance)
                        {
                            primaryHit = results[i];
                            endPos = results[i].collider.transform.position;
                            currentShortestDistance = newDistance;
                        }
                    }
                }
                linePoints = new Vector3[] { handRayDirection.VisualAimPosition, endPos };
            }
            else
            {
                primaryHit = new RaycastHit();
                linePoints = new Vector3[] { handRayDirection.VisualAimPosition, handRayDirection.VisualAimPosition + (handRayDirection.Direction * maxRayDistance) };
            }

            numPoints = linePoints.Length;
            return hits;
        }
    }
}

public static class ConeCastExtension
{
    public static int ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle, LayerMask layerMask, out RaycastHit[] results)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance, layerMask);
        List<RaycastHit> coneCastHits = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                if (angleToHit < coneAngle)
                {
                    coneCastHits.Add(sphereCastHits[i]);
                }
            }
        }

        results = coneCastHits.ToArray();
        return coneCastHits.Count;
    }
}