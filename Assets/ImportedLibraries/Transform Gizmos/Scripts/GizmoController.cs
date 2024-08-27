using UnityEngine;

namespace TransformGizmos
{
    public class GizmoController : Singleton<GizmoController>
    {
        [SerializeField] Rotation m_rotation;
        [SerializeField] Translation m_translation;
        [SerializeField] Scaling m_scaling;
        [SerializeField] GameObject m_rotationAppendix;


        [SerializeField] Material m_clickedMaterial;
        [SerializeField] Material m_transparentMaterial;
        [SerializeField] GameObject m_objectWithMeshes;
        [SerializeField] GameObject m_degreesText;

        [Header("Adjustable Variables")]
        [SerializeField] GameObject m_targetObject;
        [SerializeField] float m_gizmoSize = 1;
        [SerializeField] float startingPivotSize = 2f;

        Transformation m_transformation = Transformation.None;
        Transformation selectedTransformation = Transformation.Translation;

        enum Transformation
        {
            None,
            Rotation,
            Translation,
            Scale
        }

        void Start()
        {
            InitializePivots();
        }

        void Update()
        {
            transform.SetPositionAndRotation(m_targetObject.transform.position, m_targetObject.transform.rotation);
            m_degreesText.transform.position = m_targetObject.transform.position;
            m_objectWithMeshes.transform.position = m_targetObject.transform.position;
            m_rotation.SetGizmoSize(m_gizmoSize);
            m_translation.SetGizmoSize(m_gizmoSize);
            m_scaling.SetGizmoSize(m_gizmoSize);
            /*
            if (Input.GetKeyDown(KeyCode.R))
                ChangeTransformationState(Transformation.Rotation);

            if (Input.GetKeyDown(KeyCode.T))
                ChangeTransformationState(Transformation.Translation);

            if (Input.GetKeyDown(KeyCode.Z))
                ChangeTransformationState(Transformation.Scale);
            */
            
        }

        void InitializePivots() {
            transform.SetPositionAndRotation(m_targetObject.transform.position, m_targetObject.transform.rotation);
            transform.localScale = m_targetObject.transform.localScale;
            m_rotation.Initialization(m_targetObject, m_clickedMaterial, m_transparentMaterial, m_objectWithMeshes, m_degreesText, m_rotationAppendix, startingPivotSize);
            m_translation.Initialization(m_targetObject, m_clickedMaterial, m_transparentMaterial, startingPivotSize);
            m_scaling.Initialization(m_targetObject, m_clickedMaterial, m_transparentMaterial, startingPivotSize);

            ChangeTransformationState(Transformation.None);
        }

        void ChangeTransformationState(Transformation transformation) {
            m_rotation.gameObject.SetActive(false);
            m_translation.gameObject.SetActive(false);
            m_scaling.gameObject.SetActive(false);

            switch (transformation) {
                case Transformation.None:
                    break;

                case Transformation.Rotation:
                    if (m_transformation == Transformation.Rotation) {
                        print("turn off pivot in switch ");
                        print("The gameobject right now is " + m_targetObject);
                        m_transformation = Transformation.None;
                    } else {
                        m_rotation.gameObject.SetActive(true);
                        m_transformation = transformation;
                    }
                    break;

                case Transformation.Translation:
                    if (m_transformation == Transformation.Translation) {
                        print("turn off pivot in switch ");
                        print("The gameobject right now is " + m_targetObject);
                        m_transformation = Transformation.None;
                    } else {
                        m_translation.gameObject.SetActive(true);
                        m_transformation = transformation;
                    }
                    break;

                case Transformation.Scale:
                    if (m_transformation == Transformation.Scale) {
                        print("turn off pivot in switch ");
                        print("The gameobject right now is " + m_targetObject);
                        m_transformation = Transformation.None;
                    } else {
                        m_scaling.gameObject.SetActive(true);
                        m_transformation = transformation;
                    }
                    break;
            }
        }

        /*
        public void ToggleRotation()
        {
            ChangeTransformationState(Transformation.Rotation);
        }

        public void ToggleMovement()
        {
            ChangeTransformationState(Transformation.Translation);
        }

        public void ToggleScale()
        {
            ChangeTransformationState(Transformation.Scale);
        }
        */
        public void SelectGameObject(GameObject gameObject) {
            m_targetObject = gameObject;
            print("selected " + m_targetObject.name);
            print("changetransf " + selectedTransformation);
            InitializePivots();
            ChangeTransformationState(selectedTransformation);
        }

        public void DeselectGameObject() {
            print("deselected ");
               m_targetObject = null;
               ChangeTransformationState(selectedTransformation); // This will turn the pivot off and prevent a weird bug
        }

        public void SelectMovement() {
            print("SelectMovement ");
            selectedTransformation = Transformation.Translation;
            GizmoPicked();
        }

        public void SelectRotation() {
            print("SelectRotation ");
            selectedTransformation = Transformation.Rotation;
            GizmoPicked();
        }

        public void SelectScale() {
            print("SelectScale ");
            selectedTransformation = Transformation.Scale;
            GizmoPicked();
        }

        void GizmoPicked() {
            if (m_targetObject == null)
                return;
        //    InitializePivots();
            ChangeTransformationState(selectedTransformation);
        }


    }
}
