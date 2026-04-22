using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour {
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellsize = 1f;
    [SerializeField] Vector3 originPosition = Vector3.zero;
    [SerializeField] bool debug = true;

    [SerializeField] Gem gemPrefab;
    [SerializeField] GemTypeSO[] gemTypeArray;
    [SerializeField] Ease ease = Ease.InQuad;

    GridSystem2D<GridObject<Gem>> grid;

    InputReader inputReader;
    Vector2Int selectedGem = Vector2Int.one * -1;

    private void Awake() {
        inputReader = GetComponent<InputReader>();
    }

    void Start() {
        InitializeGrid();
        inputReader.Fire += OnSelectGem;
    }
    private void OnDestroy() {
        inputReader.Fire -= OnSelectGem;
    }


    void InitializeGrid() {
        grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellsize, originPosition, debug);

        for (int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                CreateGem(x, y);
            }
        }
    }

    void CreateGem(int x, int y) {
        Gem gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
        gem.SetType(gemTypeArray[Random.Range(0, gemTypeArray.Length)]);
        var gridObject = new GridObject<Gem>(grid, x, y);
        gridObject.SetValue(gem);
        grid.SetValue(x, y, gridObject);
    }

    private void OnSelectGem() {
        var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

        if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) {
            return;
        }


        if(selectedGem == gridPos) {
            DeselectGem();
        }else if(selectedGem == Vector2Int.one * -1) {
            SelectGem(gridPos);
        } else {
            StartCoroutine(RunGameLoop(selectedGem, gridPos));
        }
    }

    bool IsValidPosition(Vector2Int gridPos) => gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;
    bool IsEmptyPosition(Vector2Int gridPos) => grid.GetValue(gridPos.x, gridPos.y) == null;

    IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB) {
        yield return StartCoroutine(SwapGem(gridPosA, gridPosB));

        //Matches?
        List<Vector2Int> matches = FindMatches();

        //Make Gem Explode
        yield return StartCoroutine(ExplodeGems(matches));

        //Make gems fall
        yield return StartCoroutine(MakeGemsFall());

        //Fill empty spots
        yield return StartCoroutine(FillEmptySpots());

        DeselectGem();

        yield return null;
    }

    private IEnumerator MakeGemsFall() {
        for(var x = 0; x < width; x++) {
            for(var y = 0; y < height; y++) {
                if(grid.GetValue(x, y) == null) {
                    for(var i = y + 1; i < height; i++) {
                        if(grid.GetValue(x,i) != null) {
                            var gem = grid.GetValue(x, i).GetValue();
                            grid.SetValue(x, y, grid.GetValue(x,i));
                            grid.SetValue(x, i, null);
                            gem.transform
                                .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                .SetEase(ease);

                            //SFX play woosh sound

                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator FillEmptySpots() {
        for(var x = 0; x < width; x++) {
            for(var y = 0;y < height; y++) {
                if(grid.GetValue(x,y) == null) {
                    CreateGem(x, y);

                    //SFX play sound

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

    }

    private IEnumerator ExplodeGems(List<Vector2Int> matches) {
        //SFX play sound

        foreach(var match in matches) {
            var gem = grid.GetValue(match.x, match.y).GetValue();
            grid.SetValue(match.x, match.y, null);

            //ExplodeVFX(match);

            gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);

            yield return new WaitForSeconds(0.1f);

            gem.DestroyGem();
        }

    }

    private List<Vector2Int> FindMatches() {
        HashSet<Vector2Int> matches = new();

        //Horizontal
        for(var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var gemA = grid.GetValue(x, y);
                var gemB = grid.GetValue(x + 1, y);
                var gemC = grid.GetValue(x + 2, y);

                if (gemA == null || gemB == null || gemC == null) continue;

                if(gemA.GetValue().GetType() == gemB.GetValue().GetType() 
                    && gemB.GetValue().GetType() == gemC.GetValue().GetType()) {

                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x + 1, y));
                    matches.Add(new Vector2Int(x + 2, y));
                }
            }
        }

        //Vertical
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                var gemA = grid.GetValue(x, y);
                var gemB = grid.GetValue(x, y + 1);
                var gemC = grid.GetValue(x, y + 2);

                if (gemA == null || gemB == null || gemC == null) continue;

                if (gemA.GetValue().GetType() == gemB.GetValue().GetType()
                    && gemB.GetValue().GetType() == gemC.GetValue().GetType()) {

                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x, y + 1));
                    matches.Add(new Vector2Int(x, y + 2));
                }
            }
        }

        return new List<Vector2Int>(matches);
    }

    IEnumerator SwapGem(Vector2Int gridPosA, Vector2Int gridPosB) {
        var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
        var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

        gridObjectA.GetValue().transform
            .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
            .SetEase(ease);
        gridObjectB.GetValue().transform
            .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
            .SetEase(ease);

        grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
        grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

        yield return new WaitForSeconds(0.5f);
    }

    void DeselectGem() => selectedGem = new Vector2Int(-1, -1);

    void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

    //Init Grid
    //Read player input and swap gems

    //Start Coroutine
    //swap animation
    //Matches?
    //Make gem explode
    //Replace empty spot
    //Is game over?
}
