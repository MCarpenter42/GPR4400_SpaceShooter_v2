using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This is definitely the biggest script in this whole project

public class Player : CoreFunc
{
    #region [ PARAMETERS ]

    // CAMERA

    private Camera mainCam;
    private Vector3 screenCentre;

    [SerializeField] float deadzoneScale = 1.0f;
    private float deadzoneSize;

    // UI

    private GameObject uiCanvas;

    private GameObject crosshair_Static;
    private GameObject crosshair_Dynamic;

    [SerializeField] Indicator indSprint;
    [SerializeField] Indicator indRotLock;

    // MOVEMENT

    private Rigidbody rb;

    [SerializeField] float maxMoveSpeed = 1.0f;
    private float moveForceFactor = 2.5f;
    private Vector3 movementInput = new Vector3();
    [SerializeField] float sprintFactor = 4.0f;

    private Vector3 fixedUpdateVel;
    private float dangerSpeed = 100.0f;

    // STEERING & AIMING

    [SerializeField] float maxRotSpeed = 1.0f;
    [SerializeField] float aimRotFactor = 0.3f;
    [SerializeField] bool invertPitch = false;
    private int pitchInv = -1;
    private bool lockRot = false;
    private Vector3 rotScaleVector;
    private float rotTorqueFactor = 1.0f;

    private Coroutine fovTransition;
    private float fovNormal = 60.0f;
    private float fovAiming = 50.0f;
    private float fovSprint = 75.0f;
    private bool isAiming = false;

    // WEAPONS

    private List<GameObject> weapons = new List<GameObject>();
    private List<GameObject> weaponEmitters = new List<GameObject>();
    [SerializeField] GameObject laser;

    // RESOURCES

    private int healthMax = 20;
    private int healthCurrent;

    private int energyMax = 50;
    private int energyCurrent;
    [SerializeField] float energyDrainInterval;
    private float energyDrainTimer = 0.0f;

    private GameObject barHealth;
    private Vector3 barHealth_nmSize;
    private Vector3 barHealth_nmPos;

    private GameObject barEnergy;
    private Vector3 barEnergy_nmSize;
    private Vector3 barEnergy_nmPos;

    // LEVEL

    private List<GameObject> powerCells = new List<GameObject>();
    private int cellCount;
    private int cellsObtained = 0;

    [SerializeField] TextMeshProUGUI cellCounter;

    [SerializeField] GameEnd gameEndHandler;

    // AUDIO

    [SerializeField] AudioSource thrustersSFX;
    float thrustPitch_Idle = 1.4f;
    float thrustPitchUp_Normal = 0.5f;
    float thrustPitchUp_Sprint = 0.9f;
    float thrustVolume_Idle = 0.05f;
    float thrustVolumeUp_Normal = 0.03f;
    float thrustVolumeUp_Sprint = 0.02f;

    [SerializeField] AudioSource collisionsSFX;
    [SerializeField] AudioClip collisionSlow;
    [SerializeField] AudioClip collisionFast;
    [SerializeField] AudioClip collisionRock;

    [SerializeField] AudioSource weaponsSFX;
    [SerializeField] AudioClip weaponFire;

    [SerializeField] AudioSource levelSFX;
    [SerializeField] AudioClip pickup_powerCell;
    [SerializeField] AudioClip level_success;
    [SerializeField] AudioClip level_fail1;
    [SerializeField] AudioClip level_fail2;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        mainCam = Camera.main;
        float xPos = mainCam.pixelWidth / 2.0f;
        float yPos = mainCam.pixelHeight / 2.0f;
        screenCentre = new Vector3(xPos, yPos, 0.0f);

        GetUIComponents();
        SetDeadzone();

        rb = gameObject.GetComponent<Rigidbody>();
        if (invertPitch)
        {
            pitchInv = 1;
        }

        dangerSpeed = maxMoveSpeed * sprintFactor * 0.8f;
    }

    void Start()
    {
        GetWeapons();
        ResetResources();
        GetCells();
    }

    void Update()
    {
        if (!gameState.isPaused)
        {
            DynamicCrosshair();
            Steering();
            Movement();
            Shooting();
            EnergyDrain();
            if (healthCurrent == 0 || energyCurrent == 0)
            {
                gameEndHandler.EndGame(false);
            }
        }
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(movementInput * maxMoveSpeed * Sprint());
        if (!(movementInput[0] == 0.0f && movementInput[1] == 0.0f && movementInput[2] == 0.0f))
        {
            CapSpeeds();
        }
        fixedUpdateVel = rb.velocity;

        rb.AddRelativeTorque(rotScaleVector * maxRotSpeed * rotTorqueFactor);

        ThrusterSFXControl();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fixedUpdateVel.magnitude > dangerSpeed)
        {
            if (!collision.gameObject.CompareTag("Boundary"))
            {
                TakeDamage(1);
            }
        }

        if (!collision.gameObject.CompareTag("Boundary"))
        {
            CollisionSound(collision.gameObject, fixedUpdateVel.magnitude);
        }
    }

    void OnTriggerEnter(Collider trigger)
    {
        if (trigger.gameObject.CompareTag("Pickup"))
        {
            int triggerType = (int)trigger.gameObject.GetComponent<Pickup>().type;
            int triggerPower = trigger.gameObject.GetComponent<Pickup>().power;
            Pickup(triggerType, triggerPower);
            Destroy(trigger.gameObject, 0.1f);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // Sets the radius of the area in the middle of the screen
    // where the cursor doesn't make the player rotate.
    private void SetDeadzone()
    {
        Vector3 refScale = uiCanvas.GetComponent<CanvasScaler>().referenceResolution;
        float baseScale = 0.2f;
        float newScale = baseScale * deadzoneScale / 2;
        float rectScale = refScale[1] * newScale;
        deadzoneSize = screenCentre[1] * newScale;
        crosshair_Static.GetComponent<RectTransform>().sizeDelta = new Vector2(rectScale, rectScale);
        crosshair_Dynamic.GetComponent<RectTransform>().sizeDelta = new Vector2(rectScale, rectScale);
    }

    // Retrieves references for certain UI elements.
    private void GetUIComponents()
    {
        uiCanvas = GameObject.FindGameObjectWithTag("UI");

        GameObject crosshair = GameObject.FindGameObjectWithTag("Crosshair");
        for (int i = 0; i < crosshair.transform.childCount; i++)
        {
            GameObject obj = crosshair.transform.GetChild(i).gameObject;
            if (obj.CompareTag("Static"))
            {
                
                crosshair_Static = obj;
            }
            else if (obj.CompareTag("Dynamic"))
            {
                crosshair_Dynamic = obj;
            }
        }

        barHealth = GameObject.FindGameObjectWithTag("Health");
        barHealth = GetChildrenWithTag(barHealth, "Bar")[0];
        barHealth_nmSize = barHealth.GetComponent<RectTransform>().sizeDelta;
        barHealth_nmPos = barHealth.transform.localPosition;

        barEnergy = GameObject.FindGameObjectWithTag("Energy");
        barEnergy = GetChildrenWithTag(barEnergy, "Bar")[0];
        barEnergy_nmSize = barEnergy.GetComponent<RectTransform>().sizeDelta;
        barEnergy_nmPos = barEnergy.transform.localPosition;
    }

    // Retrieves references for the weapon child objects.
    private void GetWeapons()
    {
        GameObject weaponsParent = new GameObject();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject target = gameObject.transform.GetChild(i).gameObject;
            if (target.CompareTag("Weapon"))
            {
                weaponsParent = target;
            }
        }
        for (int i = 0; i < weaponsParent.transform.childCount; i++)
        {
            GameObject target = weaponsParent.transform.GetChild(i).gameObject;
            weapons.Add(target);
        }

        foreach (GameObject weapon in weapons)
        {
            GameObject emitter = new GameObject();
            for (int i = 0; i < weapon.transform.childCount; i++)
            {
                GameObject target = weapon.transform.GetChild(i).gameObject;
                if (target.CompareTag("Emitter"))
                {
                    weaponEmitters.Add(target);
                }
            }
        }
    }

    // Retrieves information about the power cell pickups.
    private void GetCells()
    {
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject item in pickups)
        {
            if ((int)item.GetComponent<Pickup>().type == 0)
            {
                powerCells.Add(item);
            }
        }
        cellCount = powerCells.Count;
        UpdateCellCount();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // Resets Health and Energy, and their associated bars.
    private void ResetResources()
    {
        healthCurrent = healthMax;
        energyCurrent = energyMax;
        ResetBar(true);
        ResetBar(false);
    }

    // Makes the cynamic section of the crosshair conform to the cursor.
    private void DynamicCrosshair()
    {
        crosshair_Dynamic.transform.position = Input.mousePosition;
    }

    // Converts mouse position to steering control.
    private void Steering()
    {
        if (Input.GetKeyDown(controls.movement.lockRot))
        {
            ToggleRotLock();
        }

        float trueDisp = RelativeCursorPos().magnitude;
        float adjustedDisp = trueDisp - deadzoneSize;
        if (trueDisp > deadzoneSize && !lockRot)
        {
            if (isAiming)
            {
                rotTorqueFactor = aimRotFactor;
            }

            Vector3 adjustedCursorPos = RelativeCursorPos() * (adjustedDisp / trueDisp);

            float rotScalePitch = adjustedCursorPos[1] / (screenCentre[1] - deadzoneSize) * pitchInv;
            float rotScaleYaw = adjustedCursorPos[0] / (screenCentre[0] - deadzoneSize);

            rotScaleVector = new Vector3(rotScalePitch, rotScaleYaw, 0.0f);
        }

        PivotWeapons();
    }

    // Converts keyboard inputs to movement control.
    private void Movement()
    {
        movementInput = Vector3.zero;
        if (Input.GetKey(controls.movement.forward))
        {
            movementInput[2] += moveForceFactor;
        }
        if (Input.GetKey(controls.movement.back))
        {
            movementInput[2] -= moveForceFactor;
        }
        if (Input.GetKey(controls.movement.right))
        {
            movementInput[0] += moveForceFactor;
        }
        if (Input.GetKey(controls.movement.left))
        {
            movementInput[0] -= moveForceFactor;
        }
        if (Input.GetKey(controls.movement.up))
        {
            movementInput[1] += moveForceFactor;
        }
        if (Input.GetKey(controls.movement.down))
        {
            movementInput[1] -= moveForceFactor;
        }
    }

    // Handles firing inputs.
    private void Shooting()
    {
        if (Input.GetKeyDown(controls.action.aim))
        {
            isAiming = true;
            TransitionFOV(fovAiming);
        }
        else if (Input.GetKeyUp(controls.action.aim))
        {
            isAiming = false;
            TransitionFOV(fovNormal);
        }

        if (Input.GetKeyDown(controls.action.fire))
        {
            Fire();
        }
    }

    // Handles the over-time decrease of the Energy resource.
    private void EnergyDrain()
    {
        if (energyCurrent > 0)
        {
            int energyPrevious = energyCurrent;
            if (energyDrainTimer >= energyDrainInterval)
            {
                energyCurrent -= 1;
                energyDrainTimer = 0.0f;
                UpdateBar(false, energyPrevious, energyCurrent, energyMax);
            }
            energyDrainTimer += Time.deltaTime;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // OnPause and OnResume exist pretty much just to allow other
    // scripts to pause and play the thruster audio.
    public void OnPause()
    {
        thrustersSFX.Pause();
    }
    
    public void OnResume()
    {
        thrustersSFX.Play();
    }

    // Gets the cursor position relative to the centre of the screen.
    private Vector3 RelativeCursorPos()
    {
        return Input.mousePosition - screenCentre;
    }

    // Determines the vector relative to the player's facing vector
    // that is represented by the cursor position.
    private Vector3 AimVector(bool isRelative)
    {
        Vector3 aimVct_Relative = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 aimVct_Actual = new Vector3();

        aimVct_Relative[0] = (RelativeCursorPos()[0] / screenCentre[1]) * (Mathf.Sqrt(3.0f) / 3.0f);
        aimVct_Relative[1] = (RelativeCursorPos()[1] / screenCentre[1]) * (Mathf.Sqrt(3.0f) / 3.0f);
        aimVct_Relative = aimVct_Relative.normalized;

        aimVct_Actual = transform.TransformVector(aimVct_Relative);
        
        if (isRelative)
        {
            return aimVct_Relative;
        }
        else
        {
            return aimVct_Actual;
        }
    }
    
    // Returns a different value based on whether the sprint key is
    // being held, for use with movement-related functions.
    private float Sprint()
    {
        if (Input.GetKey(controls.movement.sprint) && !isAiming && movementInput.magnitude > 0.01f)
        {
            TransitionFOV(fovSprint);
            indSprint.ActiveInd(true);
            return sprintFactor;
        }
        else
        {
            if (!isAiming)
            {
                TransitionFOV(fovNormal);
            }
            indSprint.ActiveInd(false);
            return 1.0f;
        }
    }

    // Caps the magnitude of the player's linear velocity, and the
    // individual components of their rotational velocity.
    private void CapSpeeds()
    {
        Vector3 capVel = new Vector3();
        Vector3 capRot = new Vector3();
        if (rb.velocity.magnitude > maxMoveSpeed * Sprint())
        {
            capVel = rb.velocity.normalized * maxMoveSpeed * Sprint();
            rb.velocity = capVel;
        }
        if (rb.angularVelocity[0] > maxRotSpeed || rb.angularVelocity[1] > maxRotSpeed)
        {
            float xRot = rb.angularVelocity[0];
            float yRot = rb.angularVelocity[1];
            float zRot = rb.angularVelocity[2];
            Mathf.Clamp(xRot, -maxRotSpeed, maxRotSpeed);
            Mathf.Clamp(yRot, -maxRotSpeed, maxRotSpeed);
            capRot = new Vector3(xRot, yRot, zRot);
        }

    }

    // Handles locking of the player's steering controls.
    private void ToggleRotLock()
    {
        lockRot = !lockRot;
        indRotLock.ActiveInd(lockRot);
    }

    // Orients the weapon models with the aim vector.
    private void PivotWeapons()
    {
        Vector3 aim = AimVector(true);
        float pitch = -ToDeg(Mathf.Atan(aim.y));
        float yaw = ToDeg(Mathf.Atan(aim.x));

        foreach (GameObject weapon in weapons)
        {
            float yFactor = 1.0f;
            if (weapon.transform.localPosition[0] > 0)
            {
                yFactor += 0.22f;
            }
            else if (weapon.transform.localPosition[0] < 0)
            {
                yFactor += 0.22f;
            }
            weapon.transform.localEulerAngles = new Vector3(pitch, yaw * yFactor, 0.0f);
        }
    }

    // Smoothly transitions player FoV between two values.
    private void TransitionFOV(float fovTarget)
    {
        if (fovTransition != null)
        {
            StopCoroutine(fovTransition);
        }
        fovTransition = StartCoroutine(AnimTransitionFOV(fovTarget));
    }

    private IEnumerator AnimTransitionFOV(float fovTarget)
    {
        float fovStart = mainCam.fieldOfView;
        float fovDiffMax = fovNormal - fovAiming;
        float fovDiffCurrent = Mathf.Abs(fovStart - fovTarget);

        int aFrames = (int)(50.0f * fovDiffCurrent / fovDiffMax);

        for (int i = 1; i <= aFrames; i++)
        {
            float delta = (float)i / (float)aFrames;
            yield return new WaitForSeconds(0.004f);
            mainCam.fieldOfView = Mathf.Lerp(fovStart, fovTarget, delta);
        }
    }

    // Handles weapon firing.
    private void Fire()
    {
        Vector3 pos = transform.position;
        Vector3 facing = AimVector(false);
        RaycastHit hit;
        float range = 100.0f;
        if (Physics.Raycast(pos, facing, out hit, range))
        {
            Vector3 hitLocation = hit.point;

            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Breakable"))
            {
                Breakable breakObj = hitObject.GetComponent<Breakable>();
                breakObj.Break(hitLocation);
            }

            foreach (GameObject emitter in weaponEmitters)
            {
                Vector3 emitPos = emitter.transform.position;
                SpawnLaser(emitPos, hitLocation);
            }
        }
        else
        {
            Vector3 endpoint = facing * range;
            foreach (GameObject emitter in weaponEmitters)
            {
                Vector3 emitPos = emitter.transform.position;
                SpawnLaser(emitPos, endpoint);
            }
        }
        weaponsSFX.PlayOneShot(weaponFire);
    }

    // Instantiates a LineRenderer to represent a laser.
    private void SpawnLaser(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 relVector = endPoint - startPoint;
        Vector3 intermed1 = startPoint + relVector * 0.01f;
        Vector3 intermed2 = startPoint + relVector * 0.10f;
        Vector3[] linePoints = new Vector3[] { startPoint, intermed1, intermed2, endPoint };

        GameObject laserInstance = Instantiate(laser);
        LineRenderer laserRenderer = laserInstance.GetComponent<LineRenderer>();
        laserRenderer.SetPositions(linePoints);
        StartCoroutine(LaserFade(laserInstance, 0.1f));
    }

    // Handles the fade-over-time effect of the laser lines.
    private IEnumerator LaserFade(GameObject line, float time)
    {
        LineRenderer lRndr = line.GetComponent<LineRenderer>();
        Gradient lGrad = new Gradient();

        float aFrameTime = 0.01f;
        int aFrames = (int)(time / aFrameTime);

        for (int i = aFrames - 1; i >= 0; i--)
        {
            float lAlpha = (float)i / (float)aFrames;

            lGrad.SetKeys(
                lRndr.colorGradient.colorKeys,
                new GradientAlphaKey[] { new GradientAlphaKey(lAlpha, 1.0f) }
            );

            yield return new WaitForSeconds(aFrameTime);

            lRndr.colorGradient = lGrad;
        }

        Destroy(line);
    }

    // Handles taking damage.
    private void TakeDamage(int dmg)
    {
        int healthPrevious = healthCurrent;
        if (healthCurrent >= dmg)
        {
            healthCurrent -= dmg;
            UpdateBar(barHealth, healthPrevious, healthCurrent, healthMax);
        }
        else if (healthCurrent > 0)
        {
            healthCurrent = 0;
            UpdateBar(barHealth, healthPrevious, healthCurrent, healthMax);
        }
    }

    // Updates one of the player's resources bars to display the correct value,
    // with an animation where applicable.
    private void UpdateBar(bool isHealth, float valPre, float valNew, float valMax)
    {
        float scaleFactor = valNew / valMax;
        float valDif = valNew - valPre;
        if (isHealth)
        {
            GameObject mainBar = barHealth.transform.GetChild(0).gameObject;
            GameObject flashSeg = barHealth.transform.GetChild(1).gameObject;
            flashSeg.SetActive(true);

            Vector3 newSize = barHealth_nmSize;
            newSize[0] = barHealth_nmSize[0] * scaleFactor;
            mainBar.GetComponent<RectTransform>().sizeDelta = newSize;
            Vector3 flashSize = barHealth_nmSize;
            flashSize[0] = barHealth_nmSize[0] * (Mathf.Abs(valDif) / valMax);
            flashSeg.GetComponent<RectTransform>().sizeDelta = flashSize;

            Vector3 newPos = mainBar.transform.localPosition;
            newPos[0] += barHealth_nmSize[0] * (valDif / valMax) * 0.5f;
            mainBar.transform.localPosition = newPos;
            Vector3 flashPos = newPos;
            flashPos[0] += (newSize[0] + flashSize[0]) / 2.0f;
            flashSeg.transform.localPosition = flashPos;

            DoColourFlash(flashSeg.GetComponent<Image>(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 0.4f, true, false);
        }
        else
        {
            Vector3 newSize = barEnergy_nmSize;
            newSize[0] = barEnergy_nmSize[0] * scaleFactor;
            barEnergy.GetComponent<RectTransform>().sizeDelta = newSize;
            
            Vector3 newPos = barEnergy.transform.localPosition;
            newPos[0] += barEnergy_nmSize[0] * (valDif / valMax) * 0.5f;
            barEnergy.transform.localPosition = newPos;
        }
    }

    // Resets a resource bar.
    private void ResetBar(bool isHealth)
    {
        if (isHealth)
        {
            GameObject mainBar = barHealth.transform.GetChild(0).gameObject;
            GameObject flashSeg = barHealth.transform.GetChild(1).gameObject;
            mainBar.GetComponent<RectTransform>().sizeDelta = barHealth_nmSize;
            mainBar.transform.localPosition = barHealth_nmPos;
            flashSeg.transform.localPosition = barHealth_nmPos;
            flashSeg.SetActive(false);
        }
        else
        {
            barEnergy.GetComponent<RectTransform>().sizeDelta = barEnergy_nmSize;
            barEnergy.transform.localPosition = barEnergy_nmPos;
        }
    }

    // Handles interaction with triggers tagged as pickups.
    private void Pickup(int type, int power)
    {
        if (type == 0)
        {
            int energyPrevious = energyCurrent;
            if (energyMax - energyCurrent >= power)
            {
                energyCurrent += power;
            }
            else
            {
                energyCurrent = energyMax;
            }
            cellsObtained += 1;
            UpdateCellCount();
            UpdateBar(false, energyPrevious, energyCurrent, energyMax);
            LevelSFX(0);
        }
        else if (type == 1)
        {
            // PLACEHOLDER
            // Health restore
        }
    }

    // Updates the UI counter for how many power cells have been collected.
    private void UpdateCellCount()
    {
        cellCounter.text = cellsObtained + " / " + cellCount;
        if (cellsObtained == cellCount)
        {
            gameEndHandler.EndGame(true);
        }
    }

    // Handles collision audio feedback.
    private void CollisionSound(GameObject collision, float speed)
    {
        if (collision.CompareTag("Rubble"))
        {
            collisionsSFX.PlayOneShot(collisionRock);
        }
        else if (speed >= dangerSpeed)
        {
            collisionsSFX.PlayOneShot(collisionFast);
        }
        else
        {
            collisionsSFX.PlayOneShot(collisionSlow);
        }
    }

    // Handles movement audio feedback.
    private void ThrusterSFXControl()
    {
        if (rb.velocity.magnitude >= 0.0f && rb.velocity.magnitude <= maxMoveSpeed)
        {
            float delta = rb.velocity.magnitude / maxMoveSpeed;

            float pitch = thrustPitch_Idle + thrustPitchUp_Normal * delta;
            thrustersSFX.pitch = pitch;
            
            float volume = thrustVolume_Idle + thrustVolumeUp_Normal * delta;
            thrustersSFX.volume = volume;
        }
        else if (rb.velocity.magnitude > maxMoveSpeed)
        {
            float delta = ( rb.velocity.magnitude - maxMoveSpeed ) / ((sprintFactor - 1.0f) * maxMoveSpeed);

            float pitch = thrustPitch_Idle + thrustPitchUp_Normal + thrustPitchUp_Sprint * delta;
            thrustersSFX.pitch = pitch;

            float volume = thrustVolume_Idle + thrustVolumeUp_Normal + thrustVolumeUp_Sprint * delta;
            thrustersSFX.volume = volume;
        }
    }

    public void LevelSFX(int id)
    {
        float pitch = 1.0f;
        float volume = 0.5f;
        switch (id)
        {
            case 0:
                pitch = 1.3f;
                levelSFX.pitch = pitch;
                levelSFX.volume = volume;
                levelSFX.PlayOneShot(pickup_powerCell);
                break;

            case 1:
                volume = 0.25f;
                levelSFX.pitch = pitch;
                levelSFX.volume = volume;
                levelSFX.PlayOneShot(level_success);
                break;

            case 2:
                StartCoroutine(GameOverAudio());
                break;

            default:
                pitch = 1.0f;
                volume = 0.5f;
                break;
        }
    }

    // Synchronises two pieces of audio for a specific
    // effect on a fail state.
    public IEnumerator GameOverAudio()
    {
        levelSFX.pitch = 1.0f;
        levelSFX.volume = 0.25f;
        levelSFX.PlayOneShot(level_fail1);
        yield return new WaitForSecondsRealtime(0.95f);
        levelSFX.pitch = 1.0f;
        levelSFX.volume = 0.4f;
        levelSFX.PlayOneShot(level_fail2);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // This is where I put testing functions during development.
}
