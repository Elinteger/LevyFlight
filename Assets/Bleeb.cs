using UnityEngine;

public class Bleeb : MonoBehaviour
{
    public float speed = 2;
    // range of vision
    public float vision = 1;
    // courage = "stepsize" into new direction
    public float courage = 1;

    // direction in which we move, based upon the position, normalized
    Vector2 direction;
    // weeeeeeeeeeeeeeeee
    Vector2 velocity;
    // new pos
    Vector2 newPosition;


    // Update is called once per frame
    void Update()
    {
        direction = Random.insideUnitCircle * courage;
        velocity = speed * direction;
        newPosition = castToV2(transform.position) + velocity * Time.deltaTime;

        transform.position = newPosition;
        
    }


    // gets rid of the z-axis of a Vector3 to create a Vector2
    private Vector2 castToV2(Vector3 v3) {
        return new Vector2(v3.x, v3.y);
    }
}
