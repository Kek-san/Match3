using UnityEngine;

public class Match3 : MonoBehaviour {
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellsize = 1f;
    [SerializeField] Vector3 originPosition = Vector3.zero;
    [SerializeField] bool debug = true;

    GridSystem2D<GridObject<Gem>> grid;

    void Start() {
        //create a grid system
        grid = GridSystem2D<GridObject<Gem>>.HorizontalGrid(width, height, cellsize, originPosition, debug);


    }
}
