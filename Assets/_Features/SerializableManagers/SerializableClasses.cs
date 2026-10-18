using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SerializableGeoMapManager {
    public GeoLocalizationData geoData;
}

[Serializable]
public class GeoLocalizationData {
    public float longtitude, latitude, elevation;
}

[Serializable]
public class SerializableMapManager {
    public SerializableMapVariant baseMap;
    public List<SerializableMapVariant> variants;
}

[Serializable]
public class SerializableMapVariant {
    public string name;
    public string modelFileHash;
    public bool isBaseMap;
    [SerializeField] private float posX, posY, posZ;
    [SerializeField] private float rotX, rotY, rotZ;

    // Property to get/set them as a Vector3 in your code
    public Vector3 Position {
        get => new Vector3(posX, posY, posZ);
        set {
            posX = value.x;
            posY = value.y;
            posZ = value.z;
        }
    }

    public Vector3 Rotation {
        get => new Vector3(rotX, rotY, rotZ);
        set {
            rotX = value.x;
            rotY = value.y;
            rotZ = value.z;
        }
    }
}

[Serializable]
public class SerializableViewPointManager {
    public List<SerializableViewPoint> ViewPoints;
}

[Serializable]
public class SerializableViewPoint {
    public string ID;
    public string Name;
    public Vector3 position;
    public Vector3 eulerRotation;
}

[Serializable]
public class SerializablePinManager {
    public List<SerializablePin> Pins;
}

[Serializable]
public class SerializablePin {
    public string ID;
    public string Name;
    public string Description;
    public Vector3 position;
}


[Serializable]
public class SerializableSun {
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
    public bool useCurrentTime; // "Aktuální datum a čas" toggle: follow the real clock instead of the fixed values above
}

[Serializable]
public class ProjectSettingsSerializable {
    public bool viewerCameraCollision;
    public bool editorCameraCollision;
    public bool multiviewAllowed;
    public bool surveyAllowed;
    public bool firstPersonAllowed;
}

[Serializable]
public class SerializableFPSManager {
    [SerializeField] private float posX, posY, posZ;

    // Player settings (hasSettings == false -> use the prefab defaults; also the case for old saves)
    [SerializeField] private bool hasSettings;
    [SerializeField] private float playerSpeed;
    [SerializeField] private bool playerJumpAllowed;
    [SerializeField] private float playerJumpStrength;
    [SerializeField] private bool playerCrouchAllowed;
    [SerializeField] private float playerHeight;

    public Vector3 SpawnPosition {
        get => new Vector3(posX, posY, posZ);
        set {
            posX = value.x;
            posY = value.y;
            posZ = value.z;
        }
    }

    public PlayerSettings Settings {
        get {
            if (!hasSettings) return null;
            return new PlayerSettings {
                speed = playerSpeed,
                jumpAllowed = playerJumpAllowed,
                jumpStrength = playerJumpStrength,
                crouchAllowed = playerCrouchAllowed,
                playerHeight = playerHeight
            };
        }
        set {
            hasSettings = value != null;
            if (value == null) return;

            playerSpeed = value.speed;
            playerJumpAllowed = value.jumpAllowed;
            playerJumpStrength = value.jumpStrength;
            playerCrouchAllowed = value.crouchAllowed;
            playerHeight = value.playerHeight;
        }
    }
}

/// <summary>
/// Plain data holder for everything the user can configure about the player.
/// </summary>
[Serializable]
public class PlayerSettings {
    public const float MinHeight = 0.5f;

    public float speed = 5f;
    public bool jumpAllowed = true;
    public float jumpStrength = 1.5f;   // jump height in meters
    public bool crouchAllowed = true;
    public float playerHeight = 2f;     // meters

    public PlayerSettings Clone() => (PlayerSettings)MemberwiseClone();

    public void Sanitize(float minHeight = MinHeight) {
        speed = Mathf.Max(0f, speed);
        jumpStrength = Mathf.Max(0f, jumpStrength);
        playerHeight = Mathf.Max(minHeight, playerHeight);
    }

    public bool ApproximatelyEquals(PlayerSettings other) {
        if (other == null) return false;
        return Mathf.Approximately(speed, other.speed)
            && jumpAllowed == other.jumpAllowed
            && Mathf.Approximately(jumpStrength, other.jumpStrength)
            && crouchAllowed == other.crouchAllowed
            && Mathf.Approximately(playerHeight, other.playerHeight);
    }
}
