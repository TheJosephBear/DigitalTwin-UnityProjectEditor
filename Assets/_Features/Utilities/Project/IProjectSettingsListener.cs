using UnityEngine;

public interface IProjectSettingsListener {
    void OnSettingsChanged(ProjectSettingsSerializable settings);
}
