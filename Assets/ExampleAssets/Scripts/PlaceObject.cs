using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.EnhancedTouch;


[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))] //para no tener ninguna Null ref, el comp. necesario 
public class PlaceObject : MonoBehaviour
{
    [SerializeField]  private GameObject prefab;
    private ARRaycastManager aRRaycastManager; 
    private ARPlaneManager aRPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); //lista donde podemos añadir usado por func. FingerDown

    private void Awake() {
        EnhancedTouchSupport.Enable(); // Se habilita la API de EnhancedTouch
        aRRaycastManager = GetComponent <ARRaycastManager>();
        aRPlaneManager = GetComponent <ARPlaneManager>();
    }

    private void OnEnable() {
        EnhancedTouch.TouchSimulation.Enable(); //para testear usando el mouse
        EnhancedTouch.EnhancedTouchSupport.Enable(); //cada vez que tocamos la pantalla llamamos a la fun. raycast
        EnhancedTouch.Touch.onFingerDown += FingerDown; //evento
    }
    private void OnDisable() {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown; //no escuchamos más al evento
    }
    private void FingerDown(EnhancedTouch.Finger finger) { //función
        if (finger.index != 0) return; //si hay dos dedos, uno será 0, no soportamos multitouch
        if(aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, 
        TrackableType.PlaneWithinPolygon)){ //llamamos a la función si se reconoce un dedo llamando al Raycast a través del plano en escena (Poly.)
            foreach(ARRaycastHit hit in hits){ //para cada touch 
                Pose pose = hit.pose;//tomamos la posición y orientación 
                GameObject obj = Instantiate(prefab, pose.position, pose.rotation);//nos permite orientarlo en la posición correcta dependiendo de la pared
                
                if (aRPlaneManager.GetPlane(trackableId: hit.trackableId).alignment == PlaneAlignment.HorizontalUp)//checkea que sea el suelo
                {
                    Vector3 position = obj.transform.position;//traemos la posición del target
                    Vector3 cameraPosition = Camera.main.transform.position; //la cámara actual es marcada como Main
                    Vector3 direction = cameraPosition - position;
                    Vector3 targetRotationEuler = Quaternion.LookRotation(forward: direction).eulerAngles;//rota un obj hacia una dirección
                    Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, obj.transform.up.normalized);
                    Quaternion targetRotation = Quaternion.Euler(scaledEuler);
                    obj.transform.rotation = obj.transform.rotation * targetRotation; //en gral esta función rota el obj para estar frente a camera.

                }

                
            }
        }

    }
}
