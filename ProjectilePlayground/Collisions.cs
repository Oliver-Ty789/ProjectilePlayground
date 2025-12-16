using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using CsvHelper.Configuration.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground
{
    internal static class Collisions 
    {
        public static bool IntersectingPolygons(Vector2 centerA,Vector2[] verticesA, Vector2 centerB, Vector2[] verticesB, out Vector2 normal, out float depth) // finds egde and normal to see if polygons overlap in an axis
        {
            normal = Vector2.Zero; // direction to push the second object out of the first object
            depth = float.MaxValue;
            for (int i = 0; i< verticesA.Length; i++)
            {
                Vector2 va = verticesA[i];
                Vector2 vb = verticesA[(i + 1) % verticesA.Length];

                Vector2 edge = va - vb;
                Vector2 axis = VectorMaths.UnitVector(new(-edge.Y, edge.X)); // finds the perpendicular vector to chosen edge

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
                Vector2 axis = VectorMaths.UnitVector(new (-edge.Y, edge.X)); // finds the perpendicular vector to chosen edge

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

            // normal is pointing in the right direction

           
            
            

            Vector2 centerA2 = GetArithmeticMeanPos(verticesA);
            Vector2 centerB2 = GetArithmeticMeanPos(verticesB);

            Vector2 direction = centerB2 - centerA2;

            // if in the wrong direction, flip the normal
            if (VectorMaths.DotProduct(direction, normal)  < 0)
            {
                normal = -normal;
            }

            

            return true;
        }

        private static Vector2 GetArithmeticMeanPos(Vector2[] vertices)
        {
            float sumX = 0f;
            float sumY = 0f;

            for (int i = 0 ; i<vertices.Length; i++)
            {
                sumX += vertices[i].X;
                sumY += vertices[i].Y;
            }
            return new (sumX / (float)vertices.Length, sumY / (float)vertices.Length);
        }

        private static void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            

            for (int i = 0;i < vertices.Length;i++)
            {
                Vector2 v = vertices[i];

                float projection = VectorMaths.DotProduct(v, axis);

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



            
            // compare direction of lamba to direction of va to vb, if opposite, we know its not a contact point so return distsq to be max

            /// this is done to see if the point is parallel with an infinite edge but beyond the bounds of the actual edge
            float outOfBounds = VectorMaths.DotProduct(lambda * edge, -edge);

            float edgeDist = VectorMaths.Length(edge) * VectorMaths.Length(edge); // compensate for increase of dot products dependent on distance

            if (outOfBounds < 0 || edgeDist < outOfBounds || VectorMaths.NearlyEqual(outOfBounds, 0f) || VectorMaths.NearlyEqual(outOfBounds, edgeDist))
            {     /// this is done to see if the point is parallel with an infinite edge but beyond the bounds of the actual edge

                distSq = float.MaxValue;
                closestP = Vector2.Zero;
            }
            else
            {
                closestP = va + lambda * edge;
                distSq = VectorMaths.Length(closestP - p) * VectorMaths.Length(closestP - p);
            }
            
            



        }

        

        
        public static void ResolveCollisionsWithRotation(RigidBody bodyA, RigidBody bodyB, Vector2 normal, Environment environment, Vector2 contact1, Vector2 contact2, int contactCount, float pixelsPerM)
        {

            var relativeVelocity = Vector2.Zero;

            var j = 0f; // scalar quantity for impulse

            var e = MathF.Min(bodyA.restitution, bodyB.restitution);

            Vector2[] contactList = {contact1, contact2};
            Vector2[] impulseList = new Vector2[2];

            Vector2[] rAList = new Vector2[2];
            Vector2[] rBList = new Vector2[2];

            Vector2 centerA = GetArithmeticMeanPos(bodyA.CollisionRect.vertices);
            Vector2 centerB = GetArithmeticMeanPos(bodyB.CollisionRect.vertices);

           
                for (int i = 0; i < contactCount; i++)
                {
                    
                
                
                    Vector2 rA = contactList[i] - centerA;
                    Vector2 rB = contactList[i] - centerB;

                    

                    Vector2 rAPerp = new (-rA.Y, rA.X);
                    Vector2 rBPerp = new(-rB.Y, rB.X);

                    rAList[i] = rA;
                    rBList[i] = rB;

                    

                    Vector2 angularLinearVelocityA = rAPerp * bodyA.angularVelocity;
                    Vector2 angularLinearVelocityB = rBPerp * bodyB.angularVelocity;

                   

                    relativeVelocity = (bodyB.linearVelocity + angularLinearVelocityB) - (angularLinearVelocityA + bodyA.linearVelocity);

                    

                    float contactVelocityMag = VectorMaths.DotProduct(relativeVelocity,  normal);

                    if (contactVelocityMag > 0f) // collisions already being resolved
                    {
                        return;
                    }

                    float rAPerpDotN = VectorMaths.DotProduct(rAPerp, normal);
                    float rBPerpDotN = VectorMaths.DotProduct(rBPerp, normal);

                    //float massDot = VectorMaths.DotProduct(normal, (bodyA.invMass + bodyB.invMass) * normal);

                    float demon =  bodyA.invMass + bodyB.invMass +
                        rAPerpDotN * rAPerpDotN * bodyA.invInertia +
                        rBPerpDotN * rBPerpDotN * bodyB.invInertia;



                    j = -(1f + e) * contactVelocityMag;

                    j /= demon;
                    
                    j /= contactCount;

                    

                    Vector2 impulse = j * normal;
                   
                    impulseList[i] = impulse;
                }
                
                for (int i = 0; i < contactCount; i++)
                {
                    Vector2 impulse = impulseList[i];

                    bodyA.linearVelocity += -impulse * bodyA.invMass;
                    bodyB.linearVelocity += impulse * bodyB.invMass;

                    

                    if (!(bodyB.shapeType == ShapeType.Circle))
                        bodyB.angularVelocity -= VectorMaths.CrossProductArea(impulse, rBList[i]) * bodyB.invInertia;

                    if (!(bodyA.shapeType == ShapeType.Circle))
                        bodyA.angularVelocity += VectorMaths.CrossProductArea(impulse, rAList[i]) * bodyA.invInertia;
                    

                   

                }
            bodyA.collisionCount += 1;
            bodyB.collisionCount += 1;

            if (bodyA.collisionCount == 2)
            {
                bodyA.isTrail = false;
            }

            if (bodyB.collisionCount == 2)
            {
                bodyB.isTrail = false;

            }
             return;

               
        }
        public static void ResolveCollisionsBasic(RigidBody bodyA, RigidBody bodyB, Vector2 normal, Environment environment, Vector2 contact1, Vector2 contact2, int contactCount, float depth)
        {
            var relativeVelocity = bodyB.linearVelocity - bodyA.linearVelocity;

            var j = 0f; // scalar quantity for impulse

            var e = MathF.Min(bodyA.restitution, bodyB.restitution);

            if (VectorMaths.DotProduct(relativeVelocity, normal) > 0) // collisions already resolved
            {
                return;
            }



            var numerator = -(1 + e) * VectorMaths.DotProduct(normal, relativeVelocity);
            var denomExtension = normal * ((1 / bodyA.mass) + (1 / bodyB.mass));
            var denom = VectorMaths.DotProduct(normal, denomExtension);

            j = numerator / denom;


            bodyA.collisionCount += 1;
            bodyB.collisionCount += 1;

            if (bodyA.collisionCount == 2)
            {
                bodyA.isTrail = false;
            }

            if (bodyB.collisionCount == 2)
            {
                bodyB.isTrail = false;
            }

            bodyA.linearVelocity -= j * bodyA.invMass * normal;
            bodyB.linearVelocity += j * bodyB.invMass * normal;


            // as rotation is locked in this subroutine
            bodyA.angularVelocity = 0f;
            bodyB.angularVelocity = 0f;







            if (!bodyA.isFrictionless)
            {


                // static friction


                Vector2 frictionNormalA = normal * (environment.gravity * bodyA.mass * MathF.Cos(bodyA.rotation)); // find mag and direction of friction normal
                Vector2 frictionParallelA = new Vector2(-frictionNormalA.Y, frictionNormalA.X); // make direction parallel to contact surfaces
                bodyA.staticFriction = frictionParallelA * bodyA.staticFrictionCoefficient;

                Vector2 frictionNormalB = normal * (environment.gravity * bodyB.mass * MathF.Cos(bodyB.rotation)); // find mag and direction of friction normal
                Vector2 frictionParallelB = new Vector2(-frictionNormalB.Y, frictionNormalB.X);
                bodyB.staticFriction = (frictionParallelB * bodyB.staticFrictionCoefficient);

                // dynamic friction
                // need to be opposite to linear velocity
                Vector2 unitOppositeLinearVelocityA = new Vector2(-1, -1) * VectorMaths.UnitVector(bodyA.linearVelocity);
                Vector2 unitOppositeLinearVelcoityB = new Vector2(-1, -1) * VectorMaths.UnitVector(bodyB.linearVelocity);

                bodyA.dynamicFriction = frictionParallelA * bodyA.dynamicFrictionCoefficient * unitOppositeLinearVelocityA;
                bodyB.dynamicFriction = frictionParallelB * bodyB.dynamicFrictionCoefficient * unitOppositeLinearVelcoityB;
            }


        }

        public static void ResolveCollisionsWithRotationAndFriction(RigidBody bodyA, RigidBody bodyB, Vector2 normal, Environment environment, Vector2 contact1, Vector2 contact2, int contactCount, float pixelsPerM)
        {

            var relativeVelocity = Vector2.Zero;

            var e = MathF.Min(bodyA.restitution, bodyB.restitution);

            float staticFrictionCoefficient = bodyA.staticFrictionCoefficient;
            float dyncamicFrictionCoefficient = bodyA.dynamicFrictionCoefficient;

            Vector2[] contactList = { contact1, contact2 };
            Vector2[] impulseList = new Vector2[2];
            Vector2[] frictionImpulseList = new Vector2[2];

            Vector2[] rAList = new Vector2[2];
            Vector2[] rBList = new Vector2[2];

            float[] jList = new float[2];

            Vector2 centerA = GetArithmeticMeanPos(bodyA.CollisionRect.vertices);
            Vector2 centerB = GetArithmeticMeanPos(bodyB.CollisionRect.vertices);


            for (int i = 0; i < contactCount; i++)
            {



                Vector2 rA = contactList[i] - centerA;
                Vector2 rB = contactList[i] - centerB;



                Vector2 rAPerp = new(-rA.Y, rA.X);
                Vector2 rBPerp = new(-rB.Y, rB.X);

                rAList[i] = rA;
                rBList[i] = rB;



                Vector2 angularLinearVelocityA = rAPerp * bodyA.angularVelocity;
                Vector2 angularLinearVelocityB = rBPerp * bodyB.angularVelocity;



                relativeVelocity = (bodyB.linearVelocity + angularLinearVelocityB) - (angularLinearVelocityA + bodyA.linearVelocity);



                float contactVelocityMag = VectorMaths.DotProduct(relativeVelocity, normal);

                if (contactVelocityMag > 0f) // collisions already being resolved
                {
                    return;
                }

                float rAPerpDotN = VectorMaths.DotProduct(rAPerp, normal);
                float rBPerpDotN = VectorMaths.DotProduct(rBPerp, normal);

                //float massDot = VectorMaths.DotProduct(normal, (bodyA.invMass + bodyB.invMass) * normal);

                float demon = bodyA.invMass + bodyB.invMass +
                    rAPerpDotN * rAPerpDotN * bodyA.invInertia +
                    rBPerpDotN * rBPerpDotN * bodyB.invInertia;



                float j = -(1f + e) * contactVelocityMag;

                j /= demon;

                j /= contactCount;

                jList[i] = j;
 
                Vector2 impulse = j * normal;

                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 impulse = impulseList[i];

                bodyA.linearVelocity += -impulse * bodyA.invMass;
                bodyB.linearVelocity += impulse * bodyB.invMass;



                if (!(bodyB.shapeType == ShapeType.Circle))
                    bodyB.angularVelocity -= VectorMaths.CrossProductArea(impulse, rBList[i]) * bodyB.invInertia;

                if (!(bodyA.shapeType == ShapeType.Circle))
                    bodyA.angularVelocity += VectorMaths.CrossProductArea(impulse, rAList[i]) * bodyA.invInertia;




            }

            for (int i = 0; i < contactCount; i++)
            {

                Vector2 rA = contactList[i] - centerA;
                Vector2 rB = contactList[i] - centerB;

                Vector2 rAPerp = new(-rA.Y, rA.X);
                Vector2 rBPerp = new(-rB.Y, rB.X);

                rAList[i] = rA;
                rBList[i] = rB;

                Vector2 angularLinearVelocityA = rAPerp * bodyA.angularVelocity;
                Vector2 angularLinearVelocityB = rBPerp * bodyB.angularVelocity;

                relativeVelocity = (bodyB.linearVelocity + angularLinearVelocityB) - (angularLinearVelocityA + bodyA.linearVelocity);

                

                Vector2 tangent = relativeVelocity - VectorMaths.DotProduct(relativeVelocity, normal) * normal; // finds tangent to normal

                if (VectorMaths.NearlyEqual(tangent, Vector2.Zero))
                {
                    continue;
                }
                else
                {
                    tangent.Normalize();
                }
                float rAPerpDotT = VectorMaths.DotProduct(rAPerp, tangent);
                float rBPerpDotT = VectorMaths.DotProduct(rBPerp, tangent);

                float demon = bodyA.invMass + bodyB.invMass +
                    rAPerpDotT * rAPerpDotT * bodyA.invInertia +
                    rBPerpDotT * rBPerpDotT * bodyB.invInertia;

                float contactVelocityMag = VectorMaths.DotProduct(relativeVelocity, tangent);

                float jT = - contactVelocityMag;

                jT /= demon;

                jT /= contactCount;

                Vector2 frictionImpulse;

                float j = jList[i];

                if (MathF.Abs(jT) <= j * staticFrictionCoefficient) // if impulse is abiding Coulombs law 
                {
                    frictionImpulse = jT * tangent;
                }
                else
                {
                    frictionImpulse = -j * tangent * dyncamicFrictionCoefficient;
                }



                 frictionImpulseList[i] = frictionImpulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 frictionImpulse = frictionImpulseList[i];

                bodyA.linearVelocity += -frictionImpulse * bodyA.invMass;
                bodyB.linearVelocity += frictionImpulse * bodyB.invMass;
                if (!(bodyB.shapeType == ShapeType.Circle))
                    bodyB.angularVelocity -= VectorMaths.CrossProductArea(frictionImpulse, rBList[i]) * bodyB.invInertia;

                if (!(bodyA.shapeType == ShapeType.Circle))
                    bodyA.angularVelocity += VectorMaths.CrossProductArea(frictionImpulse, rAList[i]) * bodyA.invInertia;
            }
            bodyA.collisionCount += 1;
            bodyB.collisionCount += 1;

            if (bodyA.collisionCount == 2)
            {
                bodyA.isTrail = false;
            }

            if (bodyB.collisionCount == 2)
            {
                bodyB.isTrail = false;

            }
            return;


        }

    }
}
