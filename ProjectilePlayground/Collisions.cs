using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground
{
    internal static class Collisions 
    {
        public static bool IntersectingPolygons(Vector2[] verticesA, Vector2[] verticesB, out Vector2 normal, out float depth) // finds egde and normal to see if polygons overlap in an axis
        {
            normal = Vector2.Zero;
            depth = float.MaxValue;
            for (int i = 0; i< verticesA.Length; i++)
            {
                Vector2 va = verticesA[i];
                Vector2 vb = verticesA[(i + 1) % verticesA.Length];

                Vector2 edge = va - vb;
                Vector2 axis = new Vector2(-edge.Y, edge.X); // finds the perpendicular vector to chosen edge

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);
                

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                float axisDepth = MathF.Min(maxB - minA, maxA - minB); // to return perpendicular normal to both polygons
                if (axisDepth < depth)
                {
                    normal = axis;
                    depth = axisDepth;
                }

            }
            for (int i = 0; i < verticesB.Length; i++)
            {
                Vector2 va = verticesB[i];
                Vector2 vb = verticesB[(i + 1) % verticesB.Length];

                Vector2 edge = va - vb;
                Vector2 axis = new Vector2(-edge.Y, edge.X); // finds the perpendicular vector to chosen edge

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    normal = axis;
                    depth = axisDepth;
                }
            }
            return true;
        }

        private static void ProjectVertices(Vector2[] vertices, Vector2 normal, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0;i < vertices.Length;i++)
            {
                Vector2 v = vertices[i];
                float projection = VectorMaths.DotProduct(v, normal);

                if (projection < min)
                {
                    min = projection;
                }
                if (projection > max)
                {
                    max = projection;
                }
            }
        }

        public static void GetContactPoints(Vector2[] verticesA, Vector2[] verticesB, out Vector2 contact1 , out Vector2 contact2, out int contactCount)
        {
            contact1 = Vector2.Zero;
            contact2 = Vector2.Zero;
            contactCount = 0;

            float minDistSq = float.MaxValue;

            for (int i = 0;  i < verticesA.Length;i++)
            {
                Vector2 p = verticesA[i];

                for (int j = 0; j < verticesB.Length; j++)
                {
                    Vector2 va = verticesB[j];
                    Vector2 vb = verticesB[(j + 1) % verticesB.Length];

                   
                    PointSegmentDistance(p, va, vb, out float distSq, out Vector2 closestP);
                    Console.WriteLine($"mindistSq: {minDistSq}");

                    if (VectorMaths.NearlyEqual(distSq, minDistSq))
                    {
                        if (!VectorMaths.NearlyEqual(closestP, contact1))
                        {
                            contactCount = 2;
                            contact2 = closestP;
                        }
                        
                    }

                    else if (distSq < minDistSq) // simplist case
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = closestP;
                    }
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                Vector2 p = verticesB[i];

                for (int j = 0; j < verticesA.Length; j++)
                {
                    Vector2 va = verticesA[j];
                    Vector2 vb = verticesA[(j + 1) % verticesA.Length];

                    PointSegmentDistance(p, va, vb, out float distSq, out Vector2 closestP);

                    if (VectorMaths.NearlyEqual(distSq, minDistSq))
                    {

                        if (!VectorMaths.NearlyEqual(closestP, contact1))
                        {
                            contactCount = 2;
                            contact2 = closestP;
                        }

                    }

                    else if (distSq < minDistSq) // simplist case
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = closestP;
                    }
                }
            }
        }

        public static void PointSegmentDistance(Vector2 p, Vector2 va, Vector2 vb, out float distSq, out Vector2 closestP)
        {


            distSq = float.MaxValue;
            closestP = Vector2.Zero;

            Vector2 edge = va - vb;

            // using further maths to find the distance along the edge for the shortest point from the inputted p
            float numerator = (va.X * edge.X - p.X * edge.X + va.Y * edge.Y - p.Y * edge.Y);
            float demon = -(edge.X * edge.X) - (edge.Y * edge.Y);

            float lambda = numerator / demon;



            //Console.WriteLine(VectorMaths.DotProduct(lambda * edge, -edge));
            // compare direction of lamba to direction of va to vb, if opposite, we know its not a contact point so return distsq to be max

            /// this is done to see if the point is parallel with an infinite edge but beyond the bounds of the actual edge
            float outOfBounds = VectorMaths.DotProduct(lambda * edge, -edge);

            float edgeDist = VectorMaths.Length(edge) * VectorMaths.Length(edge); // compensate for increase of dot products dependent on distance

            if (outOfBounds < 0 || edgeDist < outOfBounds)
            {     /// this is done to see if the point is parallel with an infinite edge but beyond the bounds of the actual edge

                distSq = float.MaxValue;
                closestP = Vector2.Zero;
            }
            else
            {
                closestP = va + lambda * edge;
                distSq = VectorMaths.Length(closestP - p) * VectorMaths.Length(closestP - p);
                Console.WriteLine($"closest point on edge: {closestP}");


                Console.WriteLine($"distsq: {distSq}");
            }
            
            



        }

        //public static void StepBodies(RigidBody bodyA, RigidBody bodyB, Vector2 normal, float depth)
        //{
        //    bodyA.position += VectorMaths.UnitVector(normal) * depth;
        //    //bodyB.position -= VectorMaths.UnitVector(normal) * depth;
        //}

        
        public static void ResolveCollisionsWithRotation(RigidBody bodyA, RigidBody bodyB, Vector2 normal, Environment environment, Vector2 contact1, Vector2 contact2, int contactCount)
        {

            var relativeVelocity = Vector2.Zero;

            var j = 0f; // scalar quantity for impulse

            var e = MathF.Min(bodyA.restitution, bodyB.restitution);

            Vector2[] contactList = {contact1, contact2};
            Vector2[] impulseList = new Vector2[2];

            Vector2[] rAList = new Vector2[2];
            Vector2[] rBList = new Vector2[2];

            if (!bodyA._collidingWith.Contains(bodyB) || !bodyB._collidingWith.Contains(bodyA)) // check to see if both bodies have already collided 
            {
                for (int i = 0; i < contactCount; i++)
                {
                    Vector2 rA = contactList[i] - bodyA.position;
                    Vector2 rB = contactList[i] - bodyB.position;

                    rAList[i] = rA;
                    rBList[i] = rB;

                    Vector2 rAPerp = new Vector2(-rA.Y, rA.X);
                    Vector2 rBPerp = new Vector2(-rB.Y, rB.X);

                    Vector2 angularLinearVelocityA = rAPerp * bodyA.angularVelocity;
                    Vector2 angularLinearVelocityB = rBPerp * bodyB.angularVelocity;

                    Vector2 unitNormal = VectorMaths.UnitVector(normal);

                    relativeVelocity = (bodyB.linearVelocity + angularLinearVelocityB) - (angularLinearVelocityA + bodyA.linearVelocity);

                    float contactVelocityMag = VectorMaths.DotProduct(relativeVelocity, unitNormal);

                    //if (contactVelocityMag > 0f) // collisions already being resolved
                    //{
                    //    return;
                    //}

                    float rAPerpDotN = VectorMaths.DotProduct(rAPerp, unitNormal);
                    float rBPerpDotN = VectorMaths.DotProduct(rBPerp, unitNormal);

                    float demon = (1 / bodyA.mass) + (1 / bodyB.mass) +
                        (rAPerpDotN * rAPerpDotN) * (1 / bodyA.rotationalInertia) +
                        (rBPerpDotN * rBPerpDotN) * (1 / bodyB.rotationalInertia);



                    j = -(1f + e) * contactVelocityMag;
                    j /= demon;
                    j /= contactCount;

                    Vector2 impulse = j * normal;
                    impulseList[i] = impulse;
                }
                if (!bodyA._collidingWith.Contains(bodyB) && !bodyB._collidingWith.Contains(bodyA)) // check to see if both bodies have already collided 
                    for (int i = 0; i < contactCount; i++)
                    {
                        Vector2 impulse = impulseList[i];

                        bodyA.linearVelocity += -impulse / bodyA.mass;
                        bodyA.angularVelocity += -VectorMaths.CrossProductArea(impulse, rAList[i]) / bodyA.rotationalInertia;
                        bodyB.linearVelocity += impulse / bodyB.mass;
                        bodyB.angularVelocity += VectorMaths.CrossProductArea(impulse, rBList[i]) / bodyB.rotationalInertia;

                        // Console.WriteLine(impulse);

                    }
                bodyA.isCollisionResolved = true;
                bodyB.isCollisionResolved = true;

                bodyA._collidingWith.Add(bodyB);
                bodyB._collidingWith.Add(bodyA);

            }

            return;

               
        }
        public static void ResolveCollisionsBasic(RigidBody bodyA, RigidBody bodyB, Vector2 normal, Environment environment, Vector2 contact1, Vector2 contact2, int contactCount)
        {
            var relativeVelocity = bodyB.linearVelocity - bodyA.linearVelocity;
            
            var j = 0f; // scalar quantity for impulse

            var e = MathF.Min(bodyA.restitution, bodyB.restitution);

            //if (VectorMaths.DotProduct(relativeVelocity, normal) > 0f)
            //{
            //    return;
            //}

            j = -(1 + e) * VectorMaths.DotProduct(normal, relativeVelocity) / VectorMaths.DotProduct(normal, normal * ((1 / bodyA.mass) + (1 / bodyB.mass)));

            if (!bodyA._collidingWith.Contains(bodyB) && !bodyB._collidingWith.Contains(bodyA)) // check to see if both bodies have already collided 
            {
                Console.WriteLine("yo");
                bodyA.linearVelocity -= (j / bodyA.mass) * normal;
                bodyB.linearVelocity += (j / bodyB.mass) * normal;

                bodyA.isCollisionResolved = true;
                bodyB.isCollisionResolved = true;

                bodyA._collidingWith.Add(bodyB);
                bodyB._collidingWith.Add(bodyA);

                return;
            }

            

            // resolve angular collisions

            //if (contactCount == 1) // only one contact point
            //{
            //    Vector2 rA = contact1 - bodyA.CollisionRect.Center;
            //    Vector2 rB = contact1 - bodyB.CollisionRect.Center;

            //    Vector2 rAPerp = new Vector2(- rA.Y, rA.X);
            //    Vector2 rBPerp = new Vector2(- rB.Y, rB.X);

            //    j = -(1 + e) * VectorMaths.DotProduct(relativeVelocity, normal) / VectorMaths.DotProduct(normal, normal * (1 / bodyA.mass + 1 / bodyB.mass)) + MathF.Pow(VectorMaths.DotProduct(rAPerp, normal), 2) / bodyA.rotationalInertia + MathF.Pow(VectorMaths.DotProduct(rBPerp, normal), 2) / bodyB.rotationalInertia;

            //    bodyA.angularVelocity += VectorMaths.DotProduct(rAPerp, j * normal) / bodyA.rotationalInertia;
            //    bodyB.angularVelocity += VectorMaths.DotProduct(rBPerp, j * normal) / bodyB.rotationalInertia;
            //}
            //else // 2 contact points trying to just do it twice, find average between distance
            //{
            //    var contactAv = new Vector2((contact1.X + contact2.X) / 2, (contact1.Y + contact2.Y) / 2);

            //    Vector2 rA = contactAv - bodyA.CollisionRect.Center;
            //    Vector2 rB = contactAv - bodyB.CollisionRect.Center;

            //    Vector2 rAPerp = new Vector2(-rA.Y, rA.X);
            //    Vector2 rBPerp = new Vector2(-rB.Y, rB.X);

            //    j = -(1 + e) * VectorMaths.DotProduct(relativeVelocity, normal) / VectorMaths.DotProduct(normal, normal * (1 / bodyA.mass + 1 / bodyB.mass)) + MathF.Pow(VectorMaths.DotProduct(rAPerp, normal), 2) / bodyA.rotationalInertia + MathF.Pow(VectorMaths.DotProduct(rBPerp, normal), 2) / bodyB.rotationalInertia;
            //    //Console.WriteLine(VectorMaths.DotProduct(rAPerp, j * VectorMaths.UnitVector(normal)) / (bodyA.rotationalInertia * 100000));

            //    bodyA.angularVelocity += (VectorMaths.DotProduct(rAPerp, j * VectorMaths.UnitVector(normal)) / bodyA.rotationalInertia) * MathHelper.Pi/180;
            //    //bodyB.angularVelocity += VectorMaths.DotProduct(rBPerp, j * VectorMaths.UnitVector(normal)) / (bodyB.rotationalInertia );





            //    //Vector2 rA = contact1 - bodyA.CollisionRect.Center;
            //    //Vector2 rB = contact1 - bodyB.CollisionRect.Center;

            //    //Vector2 rAPerp = new Vector2(-rA.Y, rA.X);
            //    //Vector2 rBPerp = new Vector2(-rB.Y, rB.X);

            //    //j = -(1 + e) * VectorMaths.DotProduct(relativeVelocity, normal) / VectorMaths.DotProduct(normal, normal * (1 / bodyA.mass + 1 / bodyB.mass)) + MathF.Pow(VectorMaths.DotProduct(rAPerp, normal), 2) / bodyA.rotationalInertia + MathF.Pow(VectorMaths.DotProduct(rBPerp, normal), 2) / bodyB.rotationalInertia;

            //    //bodyA.angularVelocity += VectorMaths.DotProduct(rAPerp, j * normal) / bodyA.rotationalInertia;
            //    ////bodyB.angularVelocity += VectorMaths.DotProduct(rBPerp, j * normal) / bodyB.rotationalInertia;

            //    //rA = contact2 - bodyA.CollisionRect.Center;
            //    //rB = contact2 - bodyB.CollisionRect.Center;

            //    //rAPerp = new Vector2(-rA.Y, rA.X);
            //    //rBPerp = new Vector2(-rB.Y, rB.X);

            //    //j = -(1 + e) * VectorMaths.DotProduct(relativeVelocity, normal) / VectorMaths.DotProduct(normal, normal * (1 / bodyA.mass + 1 / bodyB.mass)) + MathF.Pow(VectorMaths.DotProduct(rAPerp, normal), 2) / bodyA.rotationalInertia + MathF.Pow(VectorMaths.DotProduct(rBPerp, normal), 2) / bodyB.rotationalInertia;

            //    //bodyA.angularVelocity += VectorMaths.DotProduct(rAPerp, j * normal) / bodyA.rotationalInertia;
            //    ////bodyB.angularVelocity += VectorMaths.DotProduct(rBPerp, j * normal) / bodyB.rotationalInertia;
            //}






            // always just apply friction to body

            // static friction
            Vector2 unitNormal = VectorMaths.UnitVector(normal); // find direction of normal

            Vector2 frictionNormalA = unitNormal * (environment.gravity * bodyA.mass * MathF.Cos(bodyA.rotation)); // find mag and direction of friction normal
            Vector2 frictionParallelA = new Vector2(-frictionNormalA.Y, frictionNormalA.X); // make direction parallel to contact surfaces
            bodyA.staticFriction = frictionParallelA * bodyA.staticFrictionCoefficient ;
 
            Vector2 frictionNormalB = unitNormal * (environment.gravity * bodyB.mass * MathF.Cos(bodyB.rotation)); // find mag and direction of friction normal
            Vector2 frictionParallelB = new Vector2(-frictionNormalB.Y, frictionNormalB.X);
            bodyB.staticFriction = (frictionParallelB * bodyB.staticFrictionCoefficient) ;

            // dynamic friction
            // need to be opposite to linear velocity
            Vector2 unitOppositeLinearVelocityA = new Vector2(-1,-1) * VectorMaths.UnitVector(bodyA.linearVelocity);
            Vector2 unitOppositeLinearVelcoityB = new Vector2(-1,-1) * VectorMaths.UnitVector(bodyB.linearVelocity);

            bodyA.dynamicFriction = frictionParallelA * bodyA.dynamicFrictionCoefficient * unitOppositeLinearVelocityA;
            bodyB.dynamicFriction = frictionParallelB * bodyB.dynamicFrictionCoefficient * unitOppositeLinearVelcoityB;
        }




        /// maybe implement later
        //public static bool IntersectingCirclePolygon(Vector2 center, float radius, VerticesRectangle rectangle, Rectangle originalRect,  float angle)
        //{

        //    var rotatedCenter = VerticesRectangle.GetTransformedCircle(center, rectangle.Center, -angle, new Vector2(0, 0), 1f);


        //    var rectCenter = rectangle.Center;
        //    // checking left side
        
        //    // only ever need to check horizontal
        //    if (MathF.Abs(rectCenter.X - rotatedCenter.X) < MathF.Abs(rectCenter.X - (rotatedCenter.X + radius)))
        //    {

        //    }


        //    return true;
        //}

    }
}
