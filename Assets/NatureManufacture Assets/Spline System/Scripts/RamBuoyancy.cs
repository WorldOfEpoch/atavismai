using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RamBuoyancy : MonoBehaviour
{
    public float buoyancy = 30;
    public float viscosity = 2;
    public float viscosityAngular = 0.4f;

    public LayerMask layer = 16;

    public new Collider collider;

    [Range(2, 10)]
    public int pointsInAxis = 2;
    new Rigidbody rigidbody;
    static RamSpline[] ramSplines;
    static LakePolygon[] lakePolygons;

    public List<Vector3> volumePoints = new List<Vector3>();
    public bool autoGenerateVolumePoints = true;
    Vector3[] volumePointsMatrix;
    Vector3 lowestPoint;
    Vector3 center = Vector3.zero;

    public bool debug = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (ramSplines == null)
            ramSplines = FindObjectsOfType<RamSpline>();
        if (lakePolygons == null)
            lakePolygons = FindObjectsOfType<LakePolygon>();

        if (collider == null)
            collider = GetComponent<Collider>();

        if (collider == null)
        {
            Debug.LogError("Buoyancy doesn't have collider");
            this.enabled = false;
            return;
        }

        if (autoGenerateVolumePoints)
        {
            Vector3 size = collider.bounds.size;
            Vector3 min = collider.bounds.min;
            Vector3 step = new Vector3(size.x / (float)pointsInAxis, size.y / (float)pointsInAxis, size.z / (float)pointsInAxis);


            for (int x = 0; x <= pointsInAxis; x++)
            {
                for (int y = 0; y <= pointsInAxis; y++)
                {
                    for (int z = 0; z <= pointsInAxis; z++)
                    {
                        Vector3 vertice = new Vector3(min.x + x * step.x, min.y + y * step.y, min.z + z * step.z);
                        Vector3 closestPoint = collider.ClosestPoint(vertice);

                        //Debug.DrawLine(closestPoint, vertice, Color.red, 50);


                        // Debug.Log(Vector3.Distance(closestPoint, vertice));
                        if (Vector3.Distance(closestPoint, vertice) < float.Epsilon)
                        {
                            // Debug.Log(vertice);
                            volumePoints.Add(transform.InverseTransformPoint(vertice));
                        }

                    }
                }
            }
        }

        volumePointsMatrix = new Vector3[volumePoints.Count];



    }

    private void FixedUpdate()
    {

        WaterPhysics();

    }


    public void WaterPhysics()
    {
        if (volumePoints.Count == 0)
        {
            Debug.Log("Not initiated Buoyancy");
            return;
        }


        Ray ray = new Ray();
        ray.direction = Vector3.up;
        RaycastHit hit;




        bool backFace = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true;

        var thisMatrix = transform.localToWorldMatrix;

        lowestPoint = volumePoints[0];
        float minY = float.MaxValue;
        for (int i = 0; i < volumePoints.Count; i++)
        {
            volumePointsMatrix[i] = thisMatrix.MultiplyPoint3x4(volumePoints[i]);

            if (minY > volumePointsMatrix[i].y)
            {
                lowestPoint = volumePointsMatrix[i];
                minY = lowestPoint.y;
            }
        }

        ray.origin = lowestPoint;

        center = Vector3.zero;

        if (Physics.Raycast(ray, out hit, 100, layer))
        {
            float width = Mathf.Max(collider.bounds.size.x, collider.bounds.size.z);

            int verticesCount = 0;

            Vector3 velocity = rigidbody.linearVelocity;

            Vector3 velocityDirection = velocity.normalized;

            minY = hit.point.y;

            for (int i = 0; i < volumePointsMatrix.Length; i++)
            {
                if (volumePointsMatrix[i].y <= minY)
                {
                    center += volumePointsMatrix[i];
                    verticesCount++;
                }
            }
            center /= verticesCount;
            //Debug.Log(minY - center.y);
            rigidbody.AddForceAtPosition(Vector3.up * buoyancy * (minY - center.y), center);

            rigidbody.AddForce(velocity * -1 * viscosity);


            if (velocity.magnitude > 0.01f)
            {
                Vector3 v1 = Vector3.Cross(velocity, new Vector3(1, 1, 1)).normalized;

                Vector3 v2 = Vector3.Cross(velocity, v1).normalized;


                Vector3 pointFront = velocity.normalized * 10;
                Ray rayCollider;

                //int test = 0;
                RaycastHit hitCollider;

                foreach (var item in volumePointsMatrix)
                {
                    Vector3 start = pointFront + item;
                    rayCollider = new Ray(start, -velocityDirection);

                    //Debug.Log(start + " " + v1 + " " + v2);

                    //Debug.DrawRay(start, -velocityDirection * 50, Color.cyan);
                    if (collider.Raycast(rayCollider, out hitCollider, 50))
                    {
                        Vector3 pointVelocity = rigidbody.GetPointVelocity(hitCollider.point);
                        rigidbody.AddForceAtPosition(-pointVelocity * viscosityAngular, hitCollider.point);
                        //Debug.Log(hitCollider.point);
                        if (debug)
                            Debug.DrawRay(hitCollider.point, -pointVelocity * viscosityAngular, Color.red, 0.1f);
                    }
                }


                //verticesMatrix
                //for (float x = -width; x < width; x += width * 0.1f)
                //{
                //    for (float y = -width; y < width; y += width * 0.1f)
                //    {
                //        test++;
                //        Vector3 start = pointFront + (v1 * x) + (v2 * y);
                //        rayCollider = new Ray(start, -velocityDirection);

                //        //Debug.Log(start + " " + v1 + " " + v2);

                //        //Debug.DrawRay(start, -velocityDirection*50, Color.cyan);
                //        if (collider.Raycast(rayCollider, out hitCollider, 50))
                //        {
                //            Vector3 pointVelocity = rigidbody.GetPointVelocity(hitCollider.point);
                //            rigidbody.AddForceAtPosition(-pointVelocity * viscosityAngular, hitCollider.point);
                //            //Debug.Log(hitCollider.point);
                //            if (debug)
                //                Debug.DrawRay(hitCollider.point, -pointVelocity * viscosityAngular, Color.red, 0.1f);
                //        }
                //    }
                //}
                //Debug.Log(test);
            }

            RamSpline ramSpline = hit.collider.GetComponent<RamSpline>();
            LakePolygon lakePolygon = hit.collider.GetComponent<LakePolygon>();
            // In earlier versions of the NatureManufacture water system, RamSpline exposed several
            // fields such as `meshfilter`, `verticeDirection` and `floatSpeed` for driving flow
            // forces based off of the river mesh. These members have been removed in recent
            // releases, which results in a number of missing field compilation errors. To keep
            // this script compiling without those members, the entire force application based
            // on RamSpline has been removed. If you wish to re‑enable river flow forces you
            // should implement your own logic here using available API exposed by your
            // version of RamSpline (e.g. tangents, normals or custom flow data).
            if (ramSpline != null)
            {
                // Intentionally left empty – see comment above for rationale.
                if (debug)
                {
                    // Still draw debug lines so the developer can see where buoyancy and
                    // viscosity forces are being applied.
                    Debug.DrawRay(center, Vector3.up * buoyancy * (minY - center.y) * 5, Color.blue);
                    Debug.DrawRay(transform.position, velocity * -1 * viscosity * 5, Color.magenta);
                    Debug.DrawRay(transform.position, velocity * 5, Color.grey);
                    Debug.DrawRay(transform.position, rigidbody.angularVelocity * 5, Color.black);
                }
            }

            if (lakePolygon != null)
            {
                // Newer versions of LakePolygon removed several properties such as
                // `meshfilter` and `floatSpeed`. Without those members we can no longer
                // calculate a flow direction or apply additional forces to buoyancy.
                // You may implement custom behaviour here based on your own data.
                if (debug)
                {
                    // Draw a placeholder debug arrow pointing along the forward axis
                    // to indicate buoyancy direction when interacting with a lake polygon.
                    Debug.DrawRay(transform.position + Vector3.up, Vector3.forward * 5, Color.red);
                    Debug.DrawRay(center, Vector3.up * buoyancy * (minY - center.y) * 5, Color.blue);
                    Debug.DrawRay(transform.position, velocity * -1 * viscosity * 5, Color.magenta);
                    Debug.DrawRay(transform.position, velocity * 5, Color.grey);
                    Debug.DrawRay(transform.position, rigidbody.angularVelocity * 5, Color.black);
                }
            }
        }





        Physics.queriesHitBackfaces = backFace;
    }

    void OnDrawGizmosSelected()
    {
        if (!debug)
            return;


        if (collider != null && volumePointsMatrix != null)
        {

            var thisMatrix = transform.localToWorldMatrix;
            foreach (var item in volumePointsMatrix)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(item, .08f);
            }

        }

        if (lowestPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lowestPoint, .08f);
        }

        if (center != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(center, .08f);

        }
    }

}
