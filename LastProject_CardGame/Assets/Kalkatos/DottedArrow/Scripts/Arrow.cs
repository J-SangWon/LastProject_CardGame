using UnityEngine;

namespace Kalkatos.DottedArrow
{
	public class Arrow : MonoBehaviour
    {
		public Transform Origin { get { return origin; } set { origin = value; } }

		[SerializeField] private float baseHeight;
		[SerializeField] private RectTransform baseRect;
		[SerializeField] private Transform origin;
		[SerializeField] private bool startsActive;
		[SerializeField] private Transform fixedTarget; // AI용: 마우스 대신 특정 타깃을 향하도록 함

		private RectTransform myRect;
		private Canvas canvas;
		private Camera mainCamera;
		private bool isActive;

		private void Awake ()
		{
			myRect = (RectTransform)transform;
			canvas = GetComponentInParent<Canvas>();
			mainCamera = Camera.main;
			SetActive(startsActive);
		}

		private void Update ()
		{
			if (!isActive)
				return;
			Setup();

			if(Input.GetMouseButtonDown(1))	
            {
                if (baseRect.gameObject.activeSelf)
                    Deactivate();
            }
        }

		private void Setup()
		{
			if (origin == null)
				return;
            Vector2 originPosOnScreen = origin.position;
			myRect.anchoredPosition = new Vector2(originPosOnScreen.x - Screen.width / 2, originPosOnScreen.y - Screen.height / 2) / canvas.scaleFactor;
			Vector2 targetScreenPos = fixedTarget != null ? (Vector2)fixedTarget.position : (Vector2)Input.mousePosition;
			Vector2 differenceToTarget = targetScreenPos - originPosOnScreen;
			differenceToTarget.Scale(new Vector2(1f / myRect.localScale.x, 1f / myRect.localScale.y));
			transform.up = differenceToTarget;
			baseRect.anchorMax = new Vector2(baseRect.anchorMax.x, differenceToTarget.magnitude / canvas.scaleFactor / baseHeight);
		}

		private void SetActive (bool b)
		{
			isActive = b;
			if (b)
				Setup();
			baseRect.gameObject.SetActive(b);
		}

		public void Activate () => SetActive(true);
		public void Deactivate ()
		{
			SetActive(false);
			fixedTarget = null;
		}
		public void SetupAndActivate (Transform origin)
		{
			Origin = origin;
			Activate();
		}

		// AI 등에서: 원점과 타깃을 명시해 화살표가 타깃을 향하게 함
		public void SetupAndActivate (Transform origin, Transform target)
		{
			Origin = origin;
			fixedTarget = target;
			Activate();
		}
	}
}
