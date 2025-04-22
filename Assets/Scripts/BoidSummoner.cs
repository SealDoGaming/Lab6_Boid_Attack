using UnityEngine;
using UnityEngine.UIElements;

public class BoidSummoner : MonoBehaviour
{
    [SerializeField]
    Boid boidPrefab;
    [SerializeField] 
    int numBoids = 10;
    [SerializeField]
    [Tooltip("Only for sphere SummoningType")]
    float radius=1;
    [SerializeField]
    [Tooltip("Only for cube SummoningType")]
    Vector3 scale = Vector3.one;

    public enum SummoningType { Sphere, Cube}
    public enum GizmoShowType { Never, SelectedOnly, Always }
    public enum GizmoDrawType { Wire, Solid}


    [SerializeField]
    private SummoningType summoningType = SummoningType.Sphere;
    [SerializeField]
    private GizmoShowType showRegion;
    [SerializeField]
    private GizmoDrawType drawingType=GizmoDrawType.Solid;

    [SerializeField]
    private Color gizmoColor=Color.green;
    [SerializeField]
    private float gizmoOpacity = 0.3f;

    private void Awake()
    {
        for (int i = 0; i < numBoids; i++) {
            Vector3 position = Vector3.zero;
            if (summoningType == SummoningType.Sphere)
            {
                position = transform.position + Random.insideUnitSphere * radius;
            }
            else if (summoningType == SummoningType.Cube)
            {
                float x = Random.Range(-scale.x / 2, scale.x / 2);
                float y = Random.Range(-scale.y / 2, scale.y / 2);
                float z = Random.Range(-scale.z / 2, scale.z / 2);
                position = transform.position + new Vector3(x, y, z);
            }
            Vector3 dir = Random.insideUnitSphere;
            Boid boid = Instantiate(boidPrefab);
            boid.transform.position = position;
            boid.transform.forward = dir;
        }
    }
    private void OnDrawGizmos()
    {
        if (showRegion == GizmoShowType.Always)
        {
            DrawGizmos();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showRegion == GizmoShowType.SelectedOnly)
        {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoOpacity);
        if (summoningType == SummoningType.Sphere)
        {

            Gizmos.DrawWireSphere(transform.position, radius);
        }else if(summoningType == SummoningType.Cube)
        {
            Gizmos.DrawWireCube(transform.position, scale);
        }
    }
}
