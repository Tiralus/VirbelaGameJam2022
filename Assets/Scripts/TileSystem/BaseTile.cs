using UnityEngine;

public abstract class BaseTile : MonoBehaviour
{
    public Transform Selection;
    public float SelectionOscillationSpeed;
    public float EndingHeightChange;

    public bool Selected { get; set; }

    private bool _selectionOscillationDirection;
    private float _selectionOscillationTimer;
    private Vector3 _startingSelectionPosition;
    private Vector3 _endingSelectionPosition;

    protected virtual void Start()
    {
        if (Selection != null)
        {
            _startingSelectionPosition = Selection.localPosition;
            _endingSelectionPosition = _startingSelectionPosition;
            _endingSelectionPosition.y += EndingHeightChange;
        }
    }

    protected virtual void Update()
    {
        if (Selection != null)
        {
            if (Selected)
            {
                float deltaOscillationTime = Time.deltaTime * SelectionOscillationSpeed;
                
                if (_selectionOscillationDirection)
                {
                    if (_selectionOscillationTimer > 0f)
                    {
                        _selectionOscillationTimer -= deltaOscillationTime;

                        if (_selectionOscillationTimer <= 0f)
                        {
                            _selectionOscillationDirection = !_selectionOscillationDirection;
                            _selectionOscillationTimer = 0f;
                        }
                    }
                }
                else
                {
                    if (_selectionOscillationTimer < 1f)
                    {
                        _selectionOscillationTimer += deltaOscillationTime;
                        
                        if (_selectionOscillationTimer >= 1f)
                        {
                            _selectionOscillationDirection = !_selectionOscillationDirection;
                            _selectionOscillationTimer = 1f;
                        }
                    }
                }

                Selection.localPosition = Vector3.Lerp(_startingSelectionPosition, _endingSelectionPosition,
                    _selectionOscillationTimer);
            }
            else
            {
                Selection.localPosition = _startingSelectionPosition;
            }
        }
    }
    
    public virtual void Select()
    {
        Selected = true;
        _selectionOscillationTimer = 0f;
        _selectionOscillationDirection = false;
    }

    public virtual void Deselect()
    {
        Selected = false;
    }
}
