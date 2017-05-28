using UnityEngine;
using System.Collections;
using System;
using TreasureHunt;
using UnityEngine.UI;

public class LocationManager : MonoBehaviour
{
    private Location targetLocation;

    public InputField latitudeInput;
    public InputField longitudeInput;

    public event Action StartingLocationService;
    public event Action LocationDisabledByUser;
    public event Action LocationServiceNotStarted;
    public event Action LocationServiceStarted;

    public event Action TargetLocationReached;
    

    public void StopLocationService()
    {
        Input.location.Stop();       
    }

    public void UseCurrentLocation(bool useCurrentLocation)
    {
        StartCoroutine(SetCurrentLocation(useCurrentLocation));
    }

    public bool AreCoordinatesInTargetRadius(Location targetLocation)
    {
        this.targetLocation = targetLocation;

        // If this method was called that means that both fields have some values
        float latitude = float.Parse(latitudeInput.text); 
        float longitude = float.Parse(longitudeInput.text);

        float distanceToTarget = GetDistance(latitude, longitude, targetLocation.Latitude, targetLocation.Longitude);

        return IsTargetReached(distanceToTarget);
    }   

    public void CheckPosition(Location targetLocation)
    {
        this.targetLocation = targetLocation;

        StartCoroutine(CheckPosition());
    }

    private IEnumerator CheckPosition()
    {
        // If it's not running already start the service now
        if (Input.location.status != LocationServiceStatus.Running)
        {            
            yield return StartCoroutine(StartLocationService()); // If this didn't start the location service an appropriate event was raised
        }
        MonoBehaviour.print("Check position status: " + Input.location.status);
                
        float runTime = Constants.LocationServiceRunTimeInSeconds;
        float updateTime = Constants.LocationServiceUpdateTimeInSeconds;

        // If location service is not stopped manually it will run for 5minutes
        while (Input.location.status == LocationServiceStatus.Running && runTime > 0)
        {
            float distanceToTarget = GetDistance(Input.location.lastData.latitude, Input.location.lastData.longitude,
                targetLocation.Latitude, targetLocation.Longitude);
            
            // If target is reached location service will stop, if not this while loop will continue
            IsTargetReached(distanceToTarget);

            runTime -= Time.deltaTime;
            yield return new WaitForSeconds(updateTime);
        }
    }

    private IEnumerator SetCurrentLocation(bool useCurrentLocation)
    {
        // If it's not running already start the service now
        if (Input.location.status != LocationServiceStatus.Running)
        {
            // If the service doesn't start, latitude&longitude input fields won't get any values 
            yield return StartCoroutine(StartLocationService());            
        }
        MonoBehaviour.print("Use current location status: " + Input.location.status);

        float updateTime = Constants.LocationServiceUpdateTimeInSeconds;
                
        while (useCurrentLocation && Input.location.status == LocationServiceStatus.Running)
        {
            latitudeInput.text = Input.location.lastData.latitude.ToString();
            longitudeInput.text = Input.location.lastData.longitude.ToString();

            yield return new WaitForSeconds(updateTime);
        }
    }

    private IEnumerator StartLocationService()
    {
        // Check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            if (LocationDisabledByUser != null)
            {
                LocationDisabledByUser();
            }

            yield break;
        }

        // Start service before querying location
        Input.location.Start(Constants.DesiredAccuracyInMeters, Constants.UpdateDistanceInMeters);

        if (StartingLocationService != null)
        {
            StartingLocationService();
        }
        yield return new WaitForSeconds(2);

        // Wait until service initializes
        int waitTime = Constants.LocationServiceWaitTimeInSeconds;
        while (Input.location.status == LocationServiceStatus.Initializing && waitTime > 0)
        {
            yield return new WaitForSeconds(1);
            waitTime--;
        }

        // Service didn't initialize in time or the connection failed
        if (waitTime < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            if (LocationServiceNotStarted != null)
            {
                LocationServiceNotStarted();
            }

            yield break;
        }
        else
        {
            // Access granted, location service started
            if (LocationServiceStarted != null)
            {
                LocationServiceStarted();
            }
        }
    }        

    private float GetDistance(float latitudeA, float longitudeA, float latitudeB, float longitudeB)
    {
        var latitudeAInRadians = latitudeA * Mathf.Deg2Rad;
        var latitudeBInRadians = latitudeB * Mathf.Deg2Rad;
        var deltaLatitudeInRadians = Mathf.DeltaAngle(latitudeA, latitudeB) * Mathf.Deg2Rad;
        var deltaLongitudeInRadians = Mathf.DeltaAngle(longitudeA, longitudeB) * Mathf.Deg2Rad;

        // Square of half the chord length between two points
        var a = Mathf.Pow(Mathf.Sin(deltaLatitudeInRadians / 2), 2) +
            Mathf.Cos(latitudeBInRadians) * Mathf.Cos(latitudeAInRadians) * Mathf.Pow(Mathf.Sin(deltaLongitudeInRadians / 2), 2);

        // Angular distance in radians
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        // ~ 6 371km
        var earthRadius = 6371000;

        // Distance from current position to target position
        var distance = earthRadius * c;

        return distance;
    }

    private bool IsTargetReached(float distanceToTarget)
    {
        if (distanceToTarget < targetLocation.Radius / 2)
        {
            // Target reached
            if (TargetLocationReached != null)
            {
                TargetLocationReached();
            }                        
            StopLocationService();

            return true;
        }

        // Target not reached
        return false;
    }
}