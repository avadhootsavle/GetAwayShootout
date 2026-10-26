using UnityEngine;


public class CameraManager : MonoBehaviour
{
    public Camera cameraObject;
    public bool forceEqualCameraBounds;
    //public CameraUIProperties cameraUI;

    [System.Serializable]
    public class CameraUIProperties
    {
        public Canvas canvas;
        public GameObject cameraUIObject;

        public float lineWidth = 5;
        public Color lineColor = Color.white;
    }

    private Camera menuCamera;
    private AudioListener menuListener;
    void Awake()
    {
        // Overlay UI still needs a rendering camera in the Unity Game view.
        menuCamera=new GameObject("Menu Camera").AddComponent<Camera>();
        menuCamera.transform.SetParent(transform,false);
        menuListener=menuCamera.gameObject.AddComponent<AudioListener>();
        menuCamera.orthographic=true; menuCamera.cullingMask=0;
        menuCamera.clearFlags=CameraClearFlags.SolidColor;
        menuCamera.backgroundColor=PixelUI.Ink; menuCamera.depth=-100; menuCamera.targetDisplay=0;
    }

    private Camera[] cameras;
    private CameraGrid cameraGrid;

    public Camera[] Initalize(int numCameras)
    {
        if(menuCamera!=null) menuCamera.enabled=false;
        if(menuListener!=null)menuListener.enabled=false;
        cameras = new Camera[numCameras];
        cameraGrid = new CameraGrid(numCameras, forceEqualCameraBounds);

        CreateCams();
        //CreateCameraUI();

        return cameras;
    }

    void CreateCams()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera cam = Instantiate(cameraObject.gameObject, transform).GetComponent<Camera>();
            cameras[i] = cam;
            cam.backgroundColor = new Color(.16f, .75f, .86f);
            cam.GetComponent<Camera>().rect = cameraGrid.GetGridRect(i);
            cam.gameObject.AddComponent<CameraInfo>().SetCameraID(i);
            CitySky.Attach(cam, i);

            if (i == 0)
                cam.gameObject.AddComponent<AudioListener>();
        }
    }

    public CameraGrid GetCameraGrid()
    {
        return cameraGrid;
    }

    //void CreateCameraUI()
    //{
    //    Transform holder = null;
    //    if (cameraUI.canvas.gameObject.scene.name == null)
    //        holder = Instantiate(cameraUI.canvas.gameObject).transform;
    //    else
    //        holder = cameraUI.canvas.transform;

    //    for (int i = 0; i < cameras.Length; i++)
    //    {
    //        Rect rect = cameraGrid.GetGridRect(i);
    //        RectTransform rt = new GameObject("CameraUI_" + i).AddComponent<RectTransform>();
    //        rt.SetParent(holder);
    //        rt.localScale = Vector3.one;

    //        rt.anchorMin = rect.min;
    //        rt.anchorMax = rect.max;
    //        rt.offsetMin = Vector2.zero;
    //        rt.offsetMax = Vector2.zero;

    //        RectTransform rtObj = Instantiate(cameraUI.cameraUIObject, rt.transform).GetComponent<RectTransform>();
    //        rtObj.localScale = Vector3.one;
    //        rtObj.offsetMin = Vector2.zero;
    //        rtObj.offsetMax = Vector2.zero;

    //        if (cameraGrid.HasRightNeighbor(i))
    //            CreateUILine(rt.transform, true);
    //        if (cameraGrid.HasBelowNeighbor(i))
    //            CreateUILine(rt.transform, false);
    //    }
    //}

    //void CreateUILine(Transform camUIRect, bool isRight)
    //{
    //    float lw = cameraUI.lineWidth / 2;

    //    RectTransform rt = new GameObject("GridLine_" + (isRight ? "R" : "D")).AddComponent<RectTransform>();
    //    rt.SetParent(camUIRect);
    //    rt.localScale = Vector3.one;

    //    rt.anchorMin = new Vector2(isRight ? 1 : 0, 0);
    //    rt.anchorMax = new Vector2(1, isRight ? 1 : 0);
    //    rt.offsetMin = new Vector2(isRight ? -lw : 0, isRight ? 0 : -lw);
    //    rt.offsetMax = new Vector2(isRight ? lw : 0, isRight ? 0 : lw);

    //    rt.gameObject.AddComponent<Image>().color = cameraUI.lineColor;
    //}
}