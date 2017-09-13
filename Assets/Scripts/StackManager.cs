using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StackManager : MonoBehaviour
{
	public Text ScoreText;
	public Color32[] gameColors = new Color32[4];
	public Material stackMat;

	private const int RATIO = 20;
	private const float BOUNDS_SIZE = 3.5f * RATIO;
	private const float STACK_MOVING_SPEED = 5.0f;
	private const float ERROR_MARGIN = 0.5f * RATIO;
	private const float STACK_BOUNDS_GAIN = 0.25f * RATIO;
	private const int COMBO_START_GAIN = 3;

	private GameObject[] theStack;
	private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

	private int stackIndex;
	private int scoreCount = 0;
	private int combo = 0;

	private float tileTransition = 0.0f;
	private float tileSpeed = 2.5f;
	private float secondaryPosition;

	private bool isMovingOnX = true;
	private bool gameOver = false;

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;

	private void Start()
	{
		theStack = new GameObject[transform.childCount];
		for (int i = 0; i < transform.childCount; i++)
		{
			theStack[i] = transform.GetChild(i).gameObject;
			ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
		}

		stackIndex = transform.childCount - 1;
	}

	private void CreateRubble(Vector3 pos, Vector3 scale)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		go.AddComponent<Rigidbody>();

		go.GetComponent<MeshRenderer>().material = stackMat;
		ColorMesh(go.GetComponent<MeshFilter> ().mesh);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (PlaceTile())
			{
				SpawnTile();
				scoreCount++;
				ScoreText.text = scoreCount.ToString ();
			}
			else
			{
				EndGame ();
			}
		}

		MoveTile();

		// Move the stack
		transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);
	}

	private void MoveTile()
	{
		if (gameOver)
			return;

		tileTransition += Time.deltaTime * tileSpeed;
		if (isMovingOnX)
			theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount * RATIO, secondaryPosition);
		else
			theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount * RATIO, Mathf.Sin(tileTransition) * BOUNDS_SIZE);
	}

	private void SpawnTile()
	{
		lastTilePosition = theStack[stackIndex].transform.localPosition;
		stackIndex--;
		if (stackIndex < 0)
			stackIndex = transform.childCount - 1;

		desiredPosition = (Vector3.down) * scoreCount * RATIO;
		theStack[stackIndex].transform.localPosition = new Vector3(0, scoreCount * RATIO, 0);
		theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, RATIO, stackBounds.y);

		ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
	}

	private bool PlaceTile()
	{
		Transform t = theStack[stackIndex].transform;

		if (isMovingOnX)
		{
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs(deltaX) > ERROR_MARGIN)
			{
				//CUT THE TILE
				combo = 0;
				stackBounds.x -= Mathf.Abs(deltaX);
				if (stackBounds.x <= 0)
					return false;

				float middle = lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = new Vector3(stackBounds.x, RATIO, stackBounds.y);
				CreateRubble
				(
					new Vector3((deltaX < 0)
						? t.position.x + (t.localScale.x / 2)
						: t.position.x - (t.localScale.x / 2)
						, t.position.y
						, t.position.z),
					new Vector3(Mathf.Abs(deltaX) - 0.01f, RATIO, t.localScale.z)
				);
				t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount * RATIO, lastTilePosition.z);
			}
			else
			{
				if (combo > COMBO_START_GAIN)
				{
					stackBounds.x += STACK_BOUNDS_GAIN;
					if (stackBounds.x > BOUNDS_SIZE)
						stackBounds.x = BOUNDS_SIZE;

					float middle = lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = new Vector3(stackBounds.x, RATIO, stackBounds.y);
					t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount * RATIO, lastTilePosition.y);
				}

				combo++;
				t.localPosition = new Vector3(lastTilePosition.x, scoreCount * RATIO, lastTilePosition.z);
			}
		}
		else
		{
			float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
			{
				//CUT THE TILE
				combo = 0;
				stackBounds.y -= Mathf.Abs(deltaZ);
				if (stackBounds.y <= 0)
					return false;

				float middle = lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3(stackBounds.x, RATIO, stackBounds.y);
				CreateRubble
				(
					new Vector3(t.position.x
						, t.position.y
						, (deltaZ < 0)
						? t.position.z + (t.localScale.z / 2)
						: t.position.z - (t.localScale.z / 2)),
					new Vector3(t.localScale.x, RATIO, Mathf.Abs(deltaZ))
				);
				t.localPosition = new Vector3(lastTilePosition.x, scoreCount * RATIO, middle - (lastTilePosition.z / 2));
			}
			else
			{
				if (combo > COMBO_START_GAIN)
				{
					if (stackBounds.y > BOUNDS_SIZE)
						stackBounds.y = BOUNDS_SIZE;

					stackBounds.y += STACK_BOUNDS_GAIN;
					float middle = lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = new Vector3(stackBounds.x, RATIO, stackBounds.y);
					t.localPosition = new Vector3(lastTilePosition.x, scoreCount * RATIO, middle - (lastTilePosition.z / 2));
				}
				combo++;
				t.localPosition = new Vector3(lastTilePosition.x, scoreCount * RATIO, lastTilePosition.z);
			}
		}

		secondaryPosition = (isMovingOnX)
			? t.localPosition.x
			: t.localPosition.z;
		isMovingOnX = !isMovingOnX;

		return true;
	}

	private void ColorMesh(Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		float f = Mathf.Sin(scoreCount * 0.15f);

		for (int i = 0; i < vertices.Length; i++)
			colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3],f);

		mesh.colors32 = colors;
	}

	private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d,float t)
	{
		if (t < 0.33f)
			return Color.Lerp(a, b, t / 0.33f);
		else if ( t < 0.66f)
			return Color.Lerp (b, c, (t - 0.33f) / 0.33f);
		else
			return Color.Lerp (c, d, (t - 0.66f) / 0.33f);
	}

	private void EndGame()
	{
		Debug.Log("Lose");
		gameOver = true;
		theStack[stackIndex].AddComponent<Rigidbody>();
	}
}﻿