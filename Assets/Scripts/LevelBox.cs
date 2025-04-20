

using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LevelBox : MonoBehaviour
{
    public enum GizmoShowType { Never, SelectedOnly, AlwaysInEditor, EvenInGameplay }

    [SerializeField]
    private Vector3 position=Vector3.zero;
    [SerializeField]
    private bool centerAtHeightZero = true;

    [SerializeField]
    private float height = 10f;
    [SerializeField]
    private float xWidth = 10f;
    [SerializeField]
    private float zWidth = 10f;


    [SerializeField]
    private Transform wallParent;

    [SerializeField]
    private float wallThickness = 1f;



    [SerializeField]
    private GameObject wallPrefab;
    [SerializeField]
    private GameObject floorPrefab;



    [SerializeField]
    private Color outlineColor;


    [SerializeField]
    private GizmoShowType showType=GizmoShowType.AlwaysInEditor;
    [SerializeField]
    private bool drawEachWall = true;

    private bool loaded=false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        transform.position = position;
        loaded = true;
        CreateLevel();
       
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateLevel()
    {

        Vector3 roofOffset = Vector3.up * (wallThickness+height)/2;
        Vector3 roofScale = new Vector3(xWidth, wallThickness, zWidth);
        CreateWall(wallPrefab, roofOffset, roofScale, "Level Roof");
        for (int i = 0; i < 4; i++)
        {
            Vector3 wallOffset;
            Vector3 wallScale;
            //even x is wide odd x is thin. Opposite for z
            int mult = ((i & 2) == 0) ? 1 : -1; 
            if (((i & 1) == 0))
            {
                wallOffset = Vector3.forward * mult *( (zWidth + wallThickness) / 2);
                wallScale = new Vector3(xWidth , height, wallThickness);
            }
            else
            {

                wallOffset = Vector3.right * mult * ((xWidth + wallThickness) / 2);
                wallScale = new Vector3(wallThickness, height, zWidth);
            }
            CreateWall(wallPrefab, wallOffset, wallScale, "Level Wall " + (i + 1));
        }
        Vector3 floorOffset = Vector3.down * (wallThickness + height) / 2;
        Vector3 floorScale = new Vector3(xWidth , wallThickness, zWidth);
        CreateWall(floorPrefab, floorOffset, floorScale, "Level Floor");

    }
    private void CreateWall(GameObject usedPrefab, Vector3 offset, Vector3 scale, string name)
    {
        Vector3 pos = position + offset;
        if (centerAtHeightZero)
        {
            pos.y += height / 2;
        }
        GameObject wall = GameObject.Instantiate(usedPrefab,pos,Quaternion.identity,wallParent);
        wall.name = name;
        wall.transform.localScale = scale;
        
    }



    private void OnDrawGizmos()
    {
        if (showType == GizmoShowType.EvenInGameplay || (showType == GizmoShowType.AlwaysInEditor && loaded == false))
        {
            if (drawEachWall)
            {
                DrawLevel();
            }
            else
            {
                DrawGizmos();
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (showType == GizmoShowType.SelectedOnly)
        {
            if (drawEachWall)
            {
                DrawLevel();
            }
            else
            {
                DrawGizmos();
            }
        }
    }

    void DrawGizmos()
    {
        Vector3 pos = position;
        if (centerAtHeightZero)
        {
            pos.y += height / 2;
        }
        Vector3 size = new Vector3(xWidth, height, zWidth);
        Gizmos.color = outlineColor;
        Gizmos.DrawWireCube(pos, size);
    }
    private void DrawLevel()
    {
        Gizmos.color = outlineColor;
        Vector3 pos = position;
        if (centerAtHeightZero)
        {
            pos.y += height / 2;
        }
        Vector3 roofOffset = Vector3.up * (wallThickness + height) / 2;
        Vector3 roofScale = new Vector3(xWidth, wallThickness, zWidth);
        Gizmos.DrawWireCube(pos + roofOffset, roofScale);
        for (int i = 0; i < 4; i++)
        {
            Vector3 wallOffset;
            Vector3 wallScale;
            //even x is wide odd x is thin. Opposite for z
            int mult = ((i & 2) == 0) ? 1 : -1;
            if (((i & 1) == 0))
            {
                wallOffset = Vector3.forward * mult * ((zWidth + wallThickness) / 2);
                wallScale = new Vector3(xWidth, height, wallThickness);
            }
            else
            {

                wallOffset = Vector3.right * mult * ((xWidth + wallThickness) / 2);
                wallScale = new Vector3(wallThickness, height, zWidth);
            }
            Gizmos.DrawWireCube(pos+wallOffset, wallScale);
        }
        Vector3 floorOffset = Vector3.down * (wallThickness + height) / 2;
        Vector3 floorScale = new Vector3(xWidth, wallThickness, zWidth);
        Gizmos.DrawWireCube(pos+floorOffset, floorScale);

    }
}
