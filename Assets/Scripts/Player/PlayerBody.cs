using UnityEngine;

/// <summary>
/// Manages all player body compoenents and graphics.
/// </summary>
public class PlayerBody : MonoBehaviour
{
    public Transform body;
    public Rigidbody2D armPivot;

    public Transform handItemHolder;
    public Transform bodyItemHolder;

    private Rigidbody2D rb;

    public int characterIndex = 0;
    private SpriteRenderer bodyRenderer;
    private SpriteRenderer armRenderer;
    private SpriteRenderer faceRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetCharacter(characterIndex);
    }

    /// <summary>
    /// Set direction of player sprites.
    /// </summary>
    /// <param name="isRight">True if player to be facing to the right.</param>
    public void SetDirection(bool isRight)
    {
        if (isRight == body.localScale.x > 0)
            return;

        body.localScale = new Vector3(isRight ? 1 : -1, 1, 1);
        armPivot.transform.eulerAngles = new Vector3(0, 0, 180f - armPivot.transform.eulerAngles.z);
        // TODO: check why rigidbody.rotation does not work to set rotation
        // armPivot.rotation = 2 * rb.rotation - armPivot.rotation;

        HingeJoint2D h = armPivot.GetComponent<HingeJoint2D>();
        h.connectedAnchor = new Vector2(-h.connectedAnchor.x, h.connectedAnchor.y);
    }

    /// <summary>
    /// Swap all transform and rigidbody components with another player. Useful in teleporting player with another player.
    /// </summary>
    /// <param name="other">Other player body to swap with.</param>
    public void SwapWithOther(PlayerBody other)
    {
        Vector2 pos =transform.position;
        float rotation = transform.rotation.eulerAngles.z;

        Vector2 vel = rb.linearVelocity;
        float angVel = rb.angularVelocity;

        float armRotation = armPivot.transform.eulerAngles.z;
        Vector2 armVel = armPivot.linearVelocity;
        float armAngVel = armPivot.angularVelocity;

        bool scaleRight = transform.GetChild(0).localScale.x > 0;

        CopyFromOther(other);
        other.CopyFromOther(pos, rotation, vel, angVel, armRotation, armVel, armAngVel, scaleRight);
    }

    /// <summary>
    /// Copy all transform and rigidbody components along with moving all player items to this player. Note player items are moved not copied. 
    /// 
    /// Useful for creating a ragdoll clone of a real player.
    /// </summary>
    /// <param name="other"></param>
    public void SetFromOther(PlayerBody other)
    {
        CopyFromOther(other);

        // Transfer items in the players hand
        TransferChildren(other.handItemHolder, handItemHolder);

        // Transfer items on the players body
        TransferChildren(other.bodyItemHolder, bodyItemHolder);

        SetCharacter(other.characterIndex);
    }

    public void CopyFromOther(PlayerBody other)
    {
        CopyFromOther(
            other.rb.position, other.rb.rotation, 
            other.rb.linearVelocity, other.rb.angularVelocity, 
            other.armPivot.rotation, other.armPivot.linearVelocity, other.armPivot.angularVelocity, 
            other.body.localScale.x > 0
        );
    }

    /// <summary>
    /// Copy all transform and rigidbody components from a player body.
    /// </summary>
    public void CopyFromOther(Vector2 pos, float rotation, Vector2 vel, float angVel, float armRotation, Vector2 armVel, float armAngVel, bool scaleRight)
    {
        SetDirection(scaleRight);
        SetBody(pos, rotation, armRotation);

        rb.linearVelocity = vel;
        rb.angularVelocity = angVel;

        armPivot.linearVelocity = armVel;
        armPivot.angularVelocity = armAngVel;
    }

    /// <summary>
    /// Set player body to position and rotations.
    /// </summary>
    public void SetBody(Vector2 position, float rotation, float armRotation)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        armPivot.transform.rotation = Quaternion.Euler(0, 0, armRotation);
        rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0;
        armPivot.linearVelocity = Vector2.zero; armPivot.angularVelocity = 0;
    }

    void TransferChildren(Transform fromParent, Transform toParent)
    {
        for (int i = fromParent.childCount - 1; i >= 0; i--)
            fromParent.GetChild(i).SetParent(toParent, false);
    }

    public void SetCharacter(int index)
    {
        characterIndex = index;
        if (bodyRenderer == null && body != null)
            bodyRenderer = body.GetComponentInChildren<SpriteRenderer>();
        if (armRenderer == null && armPivot != null)
            armRenderer = armPivot.GetComponentInChildren<SpriteRenderer>();

        if (bodyRenderer != null) bodyRenderer.sprite = FighterArt.Body(index);
        if (armRenderer != null) armRenderer.sprite = FighterArt.Original(index, "arm");
        Sprite face = FighterArt.HasPhoto(index) ? FighterArt.Face(index) : null;
        if (faceRenderer == null && face != null && bodyRenderer != null)
        {
            var head = new GameObject("Photo face"); head.transform.SetParent(bodyRenderer.transform, false);
            faceRenderer = head.AddComponent<SpriteRenderer>();
        }
        if (faceRenderer != null)
        {
            faceRenderer.gameObject.SetActive(face != null);
            if (face != null)
            {
                faceRenderer.sprite = face;
                faceRenderer.sortingLayerID = bodyRenderer.sortingLayerID;
                faceRenderer.sortingOrder = bodyRenderer.sortingOrder + 2;
                faceRenderer.transform.localPosition = new Vector3(0, .6875f, -.01f);
                float scale = .94f / face.bounds.size.y;
                faceRenderer.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }

    /// <summary>
    /// Get rotation of player.
    /// </summary>
    /// <returns>Returns player rotation between -180f and 180f with 0f corresponding to the angle in the upright direction.</returns>
    public float GetRotation()
    {
        float rot = Mathf.Repeat(rb.rotation, 360f);
        if (rot > 180f)
            rot -= 360f;

        return rot;
    }
}
