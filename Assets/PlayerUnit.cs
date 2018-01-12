using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// A PlayerUnit is a unit controlled by a player
// This could be a character in an FPS, a zergling in a RTS
// Or a scout in a TBS

public class PlayerUnit : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    Vector3 velocity;

    // The position we think is most correct for this player.
    //   NOTE: if we are the authority, then this will be
    //   exactly the same as transform.position
    Vector3 bestGuessPosition;

    // This is a constantly updated value about our latency to the server
    // i.e. how many second it takes for us to receive a one-way message
    // TODO: This should probably be something we get from the PlayerConnectionObject
    float ourLatency;   

    // This higher this value, the faster our local position will match the best guess position
    float latencySmoothingFactor = 10;

	// Update is called once per frame
	void Update () {
		// This function runs on ALL PlayerUnits -- not just the ones that I own.

        // Code running right here is running for ALL version of this object, even
        // if it's not the authoratitive copy.
        // But even if we're NOT the owner, we are trying to PREDICT where the object
        // should be right now, based on the last velocity update.


        // How do I verify that I am allowed to mess around with this object?
        if( hasAuthority == false )
        {
            // We aren't the authority for this object, but we still need to update
            // our local position for this object based on our best guess of where
            // it probably is on the owning player's screen.

            bestGuessPosition = bestGuessPosition + ( velocity * Time.deltaTime );

            // Instead of TELEPORTING our position to the best guess's position, we
            // can smoothly lerp to it.

            transform.position = Vector3.Lerp( transform.position, bestGuessPosition, Time.deltaTime * latencySmoothingFactor);

            return;
        }


        // If we get to here, we are the authoritative owner of this object
        transform.Translate( velocity * Time.deltaTime );


        if( Input.GetKeyDown(KeyCode.Space) )
        {
            this.transform.Translate( 0, 1, 0 );
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            Destroy(gameObject);
        }

        if( /* some input */ true )
        {
            // The player is asking us to change our direction/speed (i.e. velocity)

            velocity = new Vector3(1, 0, 0);

            CmdUpdateVelocity(velocity, transform.position);
        }

	}

    [Command]
    void CmdUpdateVelocity( Vector3 v, Vector3 p)
    {
        // I am on a server
        transform.position = p;
        velocity = v;

        // If we know what our current latency is, we could do something like this:
        //  transform.position = p + (v * (thisPlayersLatencyToServer))


        // Now let the clients know the correct position of this object.
        RpcUpdateVelocity( velocity, transform.position);
    }

    [ClientRpc]
    void RpcUpdateVelocity( Vector3 v, Vector3 p )
    {
        // I am on a client

        if( hasAuthority )
        {
            // Hey, this is my own object. I "should" already have the most accurate
            // position/velocity (possibly more "Accurate") than the server
            // Depending on the game, I MIGHT want to change to patch this info
            // from the server, even though that might look a little wonky to the user.

            // Let's assume for now that we're just going to ignore the message from the server.
            return;
        }

        // I am a non-authoratative client, so I definitely need to listen to the server.

        // If we know what our current latency is, we could do something like this:
        //  transform.position = p + (v * (ourLatency))

        //transform.position = p;

        velocity = v;
        bestGuessPosition = p + (velocity * (ourLatency));


        // Now position of player one is as close as possible on all player's screens

        // IN FACT, we don't want to directly update transform.position, because then 
        // players will keep teleporting/blinking as the updates come in. It looks dumb.


    }


}
