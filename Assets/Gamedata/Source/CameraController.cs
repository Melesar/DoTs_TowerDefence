using UnityEngine;

namespace DoTs
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private float _moveSpeed = 4f;
        [SerializeField]
        private Camera _camera;

        private float _cameraHorizontalExtent;
        private float _cameraVerticalExtent;
        
        private Transform _transform;
        private Bounds _cameraBounds;
        
        private void Update()
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            var offset = new Vector3(x, y, 0f).normalized * _moveSpeed;

            var newPosition = _transform.position + offset;
            _transform.position = _cameraBounds.ClosestPoint(newPosition);
        }

        private void Awake()
        {
            _transform = _camera.transform;
            
            CalculateCameraSize();
            CalculateCameraBounds();
        }

        private void CalculateCameraBounds()
        {
            _cameraBounds = GetComponent<BoxCollider2D>().bounds;
            var boundsExtents = _cameraBounds.extents;
            boundsExtents.x -= _cameraHorizontalExtent;
            boundsExtents.y -= _cameraVerticalExtent;
            boundsExtents.z = 100f;
            _cameraBounds.extents = boundsExtents;
        }


        private void CalculateCameraSize()
        {
            var orthosize = _camera.orthographicSize;
            var aspect = _camera.aspect;

            _cameraVerticalExtent = orthosize;
            _cameraHorizontalExtent = orthosize * aspect;
        }
    }
}

