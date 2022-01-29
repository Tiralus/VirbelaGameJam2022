using UnityEngine;

public class Sun : MonoBehaviour
{
    public float RotationSpeed;

    private Transform _cachedTransform;
    private Vector3 _startingRotation;
    private float _rotationX;

    private void Start()
    {
        _cachedTransform = transform;
        _startingRotation = _cachedTransform.rotation.eulerAngles;
        _rotationX = _startingRotation.x;
    }

    private void Update()
    {
        _rotationX += RotationSpeed * Time.deltaTime;

        if (_rotationX > 360f)
        {
            _rotationX -= 360f;
        }

        Vector3 rotation = new Vector3(_rotationX, _startingRotation.y, _startingRotation.z);
        _cachedTransform.rotation = Quaternion.Euler(rotation);
    }
}