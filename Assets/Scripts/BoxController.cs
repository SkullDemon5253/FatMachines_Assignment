using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoxController : MonoBehaviour
{
    public Vector2Int CurrentCell { get; set; }
    public Vector2Int Size = Vector2Int.one;
    public List<Vector2Int> OccupiedCells { get; set; } = new List<Vector2Int>();

    [Header("Box Settings")]
    public string boxColorTag;

    [Header("Slide-Out Settings")]
    public Vector3 slideDirection = Vector3.forward;
    public float slideDistance = 1f;
    public float slideDuration = 0.5f;

    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector2Int startCell;
    private int fingerId = -1;

    void Start()
    {
        cam = Camera.main;
        CurrentCell = GridManager.Instance.WorldToCell(transform.position);
        startCell = CurrentCell;
        if (GridManager.Instance.TryOccupy(CurrentCell, this))
        {
            GridManager.Instance.SnapBoxToCell(this, CurrentCell);
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Ray ray = cam.ScreenPointToRay(touch.position);
                RaycastHit hit;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                        {
                            isDragging = true;
                            fingerId = touch.fingerId;
                            offset = transform.position - GetTouchWorldPoint(touch);
                            startCell = CurrentCell;

                            foreach (var c in OccupiedCells)
                                GridManager.Instance.ClearCell(c);
                            ClearAllMyCells();
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (isDragging && touch.fingerId == fingerId)
                        {
                            Vector3 targetPos = GetTouchWorldPoint(touch) + offset;
                            Vector2Int targetCell = GridManager.Instance.WorldToCell(targetPos);
                            var cellsToCheck = GetCells(targetCell);

                            foreach (var c in cellsToCheck)
                                if (!GridManager.Instance.IsWithinBounds(c) || GridManager.Instance.IsOccupied(c))
                                    return;

                            Vector3 snap = GridManager.Instance.CellToWorld(targetCell);
                            transform.position = new Vector3(snap.x, transform.position.y, snap.z);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (isDragging && touch.fingerId == fingerId)
                        {
                            isDragging = false;

                            Vector2Int newCell = GridManager.Instance.WorldToCell(transform.position);
                            if (GridManager.Instance.TryOccupy(newCell, this))
                            {
                                GridManager.Instance.SnapBoxToCell(this, newCell);
                                CurrentCell = newCell;
                            }
                            else
                            {
                                GridManager.Instance.SnapBoxToCell(this, startCell);
                                CurrentCell = startCell;
                            }
                        }
                        break;
                }
            }
        }
    }

    private Vector3 GetTouchWorldPoint(Touch touch)
    {
        Ray ray = cam.ScreenPointToRay(touch.position);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);
        return transform.position;
    }

    private List<Vector2Int> GetCells(Vector2Int origin)
    {
        var cells = new List<Vector2Int>();
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                cells.Add(new Vector2Int(origin.x + x, origin.y + y));
        return cells;
    }

    private void ClearAllMyCells()
    {
        foreach (var cell in GetCells(CurrentCell))
            GridManager.Instance.ClearCell(cell);
        OccupiedCells.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        string neededTag = boxColorTag + "Wall";
        if (other.tag == neededTag)
        {
            foreach (var c in OccupiedCells)
                GridManager.Instance.ClearCell(c);

            GameManager.Instance.RemoveBox(this);
            Handheld.Vibrate();

            GetComponent<Collider>().enabled = false;
            StartCoroutine(SlideAndDestroy());
        }
    }

    private IEnumerator SlideAndDestroy()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + slideDirection.normalized * slideDistance;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        Destroy(gameObject);
    }
}
