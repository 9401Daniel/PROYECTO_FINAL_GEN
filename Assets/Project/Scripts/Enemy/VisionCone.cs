using System.Collections.Generic;
using UnityEngine;

// Genera una mesh en tiempo real que representa el cono de visión del enemigo.
// Se recorta contra los muros para que visualmente tampoco "atraviese" paredes.
// Requiere un hijo con MeshFilter + MeshRenderer (asignado en viewMeshFilter).
public class VisionCone : MonoBehaviour
{
    [Header("Referencia al enemigo")]
    public EnemyAI enemyAI;          // Lee viewRadius y viewAngle desde el script de IA, para no duplicar valores
    public LayerMask obstacleMask;   // Misma layer de muros que usa el EnemyAI

    [Header("Calidad del cono")]
    public float meshResolution = 1.5f;      // Rayos por grado, más alto = más suave pero más costoso
    public int edgeResolveIterations = 4;    // Precisión al encontrar el borde de un obstáculo
    public float edgeDstThreshold = 0.5f;    // Diferencia de distancia que se considera "un borde"

    public MeshFilter viewMeshFilter; // Mesh hija donde se dibuja el cono
    private Mesh viewMesh;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    void LateUpdate()
    {
        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(enemyAI.viewAngle * meshResolution);
        float stepAngleSize = enemyAI.viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        // Lanza un rayo por cada "paso" angular dentro del cono
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - enemyAI.viewAngle / 2f + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                // Si entre dos rayos consecutivos hay un salto grande (uno pega en un muro y el otro no),
                // busca el borde exacto del obstáculo para que la mesh no se vea "cortada" feo
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // Arma la mesh en espacio local del enemigo, con el vértice 0 en su propia posición
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.up * 0.02f;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    // Lanza un rayo en una dirección y devuelve dónde pega (obstáculo o límite del radio de visión)
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, enemyAI.viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * enemyAI.viewRadius, enemyAI.viewRadius, globalAngle);
        }
    }

    // Búsqueda binaria del punto exacto donde termina un obstáculo, para bordes limpios en la mesh
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2f;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Info de un solo rayo lanzado dentro del cono
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit; point = _point; dst = _dst; angle = _angle;
        }
    }

    // Par de puntos que definen un borde detectado (donde empieza/termina un obstáculo dentro del cono)
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA; pointB = _pointB;
        }
    }
}
