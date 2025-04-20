
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

// Bu sinif, takip edilen nesnenin durumuna gore nesneyi gorunur veya gorunmez yapar.
public class DefaultTrackableEventHandler : MonoBehaviour
{
    public enum TrackingStatusFilter
    {
        Tracked,
        Tracked_ExtendedTracked,
        Tracked_ExtendedTracked_Limited
    }

    public TrackingStatusFilter StatusFilter = TrackingStatusFilter.Tracked_ExtendedTracked_Limited;
    public UnityEvent OnTargetFound;
    public UnityEvent OnTargetLost;

    protected TrackableBehaviour mTrackableBehaviour;
    protected TrackableBehaviour.Status m_PreviousStatus;
    protected TrackableBehaviour.Status m_NewStatus;
    protected TrackableBehaviour.StatusInfo m_PreviousStatusInfo;
    protected TrackableBehaviour.StatusInfo m_NewStatusInfo;
    protected bool m_CallbackReceivedOnce = false;

    protected virtual void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();

        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChanged);
            mTrackableBehaviour.RegisterOnTrackableStatusInfoChanged(OnTrackableStatusInfoChanged);
        }
    }

    protected virtual void OnDestroy()
    {
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.UnregisterOnTrackableStatusInfoChanged(OnTrackableStatusInfoChanged);
            mTrackableBehaviour.UnregisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
    }

    void OnTrackableStatusChanged(TrackableBehaviour.StatusChangeResult statusChangeResult)
    {
        m_PreviousStatus = statusChangeResult.PreviousStatus;
        m_NewStatus = statusChangeResult.NewStatus;

        Debug.LogFormat("Takip Edilen Nesne {0} -- {1}",
            mTrackableBehaviour.TrackableName,
            mTrackableBehaviour.CurrentStatusInfo);

        HandleTrackableStatusChanged();
    }

    void OnTrackableStatusInfoChanged(TrackableBehaviour.StatusInfoChangeResult statusInfoChangeResult)
    {
        m_PreviousStatusInfo = statusInfoChangeResult.PreviousStatusInfo;
        m_NewStatusInfo = statusInfoChangeResult.NewStatusInfo;

        HandleTrackableStatusInfoChanged();
    }

    protected virtual void HandleTrackableStatusChanged()
    {
        if (!ShouldBeRendered(m_PreviousStatus) && ShouldBeRendered(m_NewStatus))
        {
            OnTrackingFound();
        }
        else if (ShouldBeRendered(m_PreviousStatus) && !ShouldBeRendered(m_NewStatus))
        {
            OnTrackingLost();
        }
        else
        {
            if (!m_CallbackReceivedOnce && !ShouldBeRendered(m_NewStatus))
            {
                OnTrackingLost();
            }
        }

        m_CallbackReceivedOnce = true;
    }

    protected virtual void HandleTrackableStatusInfoChanged()
    {
        if (m_NewStatusInfo == TrackableBehaviour.StatusInfo.WRONG_SCALE)
        {
            Debug.LogFormat("Hedef {0} yanlis boyutlandirilmis gorunuyor. Bu izleme sorunlarina neden olabilir. Fiziksel nesne boyutu ile hedef boyutu ayni olmali.",
                mTrackableBehaviour.TrackableName);
        }
    }

    protected bool ShouldBeRendered(TrackableBehaviour.Status status)
    {
        if (status == TrackableBehaviour.Status.DETECTED ||
            status == TrackableBehaviour.Status.TRACKED)
        {
            return true;
        }

        if (StatusFilter == TrackingStatusFilter.Tracked_ExtendedTracked)
        {
            if (status == TrackableBehaviour.Status.EXTENDED_TRACKED)
                return true;
        }

        if (StatusFilter == TrackingStatusFilter.Tracked_ExtendedTracked_Limited)
        {
            if (status == TrackableBehaviour.Status.EXTENDED_TRACKED ||
                status == TrackableBehaviour.Status.LIMITED)
                return true;
        }

        return false;
    }

    protected virtual void OnTrackingFound()
    {
        if (mTrackableBehaviour)
        {
            var rendererComponents = mTrackableBehaviour.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mTrackableBehaviour.GetComponentsInChildren<Collider>(true);
            var canvasComponents = mTrackableBehaviour.GetComponentsInChildren<Canvas>(true);

            foreach (var component in rendererComponents)
                component.enabled = true;

            foreach (var component in colliderComponents)
                component.enabled = true;

            foreach (var component in canvasComponents)
                component.enabled = true;
        }

        if (OnTargetFound != null)
            OnTargetFound.Invoke();
    }

    protected virtual void OnTrackingLost()
    {
        if (mTrackableBehaviour)
        {
            var rendererComponents = mTrackableBehaviour.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mTrackableBehaviour.GetComponentsInChildren<Collider>(true);
            var canvasComponents = mTrackableBehaviour.GetComponentsInChildren<Canvas>(true);

            foreach (var component in rendererComponents)
                component.enabled = false;

            foreach (var component in colliderComponents)
                component.enabled = false;

            foreach (var component in canvasComponents)
                component.enabled = false;
        }

        if (OnTargetLost != null)
            OnTargetLost.Invoke();
    }
}
