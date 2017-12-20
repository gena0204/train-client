using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_1 : GameBase {

	class gDefine {
		public enum Direction{
			Up = 0,
			Down,
			Left,
			Right,
			Click
		}
	}

	private Vector2 screenPos = new Vector2();
	
	private Color[] colors = new Color[2];
	private Image[] directionImages = new Image[4];
	
	private int currentColor = 0;
	private int currentDirection = 0;

	private string[] typeCodes = new string[] {"S", "D"}; // 相反- D,  相同- S
	private string[] questionCodes = new string[] {"U", "D", "L", "R"}; // 上- U,  下- D,  左- L,  右- R

	public Game_1() : base() {
    }

	// Use this for initialization
	void Start () {
		colors[0] = new Color(70/255.0f, 103/255.0f, 255/255.0f);
		colors[1] = new Color(241/255.0f, 90/255.0f, 41/255.0f);
		directionImages[0] = transform.Find("Image_Up").GetComponent<Image>();
		directionImages[1] = transform.Find("Image_Down").GetComponent<Image>();
		directionImages[2] = transform.Find("Image_Left").GetComponent<Image>();
		directionImages[3] = transform.Find("Image_Right").GetComponent<Image>();

		CreateQuestion();
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR || UNITY_STANDALONE
			MouseInput();   // 滑鼠偵測
		#elif UNITY_ANDROID || UNITY_IOS
			MobileInput();  // 觸碰偵測
		#endif
	}

	void MouseInput() {
		if (Input.GetMouseButtonDown(0)) {
			screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}

		if (Input.GetMouseButtonUp(0)) {
			Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			HandDirection(screenPos, pos);
			//Debug.Log("mDirection: " + mDirection.ToString());
		}
	}
		
	void MobileInput () {
		if (Input.touchCount <= 0) {
			return;
		}
			
		if (Input.touchCount == 1) { // 1個手指觸碰螢幕
			if (Input.touches [0].phase == TouchPhase.Began) { // 開始觸碰
				// Debug.Log("Began");
				// 紀錄觸碰位置
				screenPos = Input.touches [0].position;
			} else if (Input.touches [0].phase == TouchPhase.Moved) { // 手指移動
				// Debug.Log("Moved");
				// 移動攝影機
				//Camera.main.transform.Translate (new Vector3 (-Input.touches [0].deltaPosition.x * Time.deltaTime, -Input.touches [0].deltaPosition.y * Time.deltaTime, 0));
			}
				
			// 手指離開螢幕
			if (Input.touches [0].phase == TouchPhase.Ended || Input.touches [0].phase == TouchPhase.Canceled) {
				HandDirection(screenPos, Input.touches [0].position);
			}
		} else if (Input.touchCount > 1) { // 攝影機縮放，如果1個手指以上觸碰螢幕
			// 記錄兩個手指位置
			Vector2 finger1 = new Vector2();
			Vector2 finger2 = new Vector2();
				
			// 記錄兩個手指移動距離
			Vector2 move1 = new Vector2();
			Vector2 move2 = new Vector2();
				
			// 是否是小於2點觸碰
			for (int i=0; i<2; i++) {
				UnityEngine.Touch touch = UnityEngine.Input.touches [i];
					
				if (touch.phase == TouchPhase.Ended)
					break;
					
				if (touch.phase == TouchPhase.Moved) {
					// 每次都重置
					float move = 0;
						
					// 觸碰一點
					if (i == 0) {
						finger1 = touch.position;
						move1 = touch.deltaPosition;
						// 另一點
					} else {
						finger2 = touch.position;
						move2 = touch.deltaPosition;
							
						// 取最大X
						if (finger1.x > finger2.x) {
							move = move1.x;
						} else {
							move = move2.x;
						}
							
						// 取最大Y，並與取出的X累加
						if (finger1.y > finger2.y) {
							move += move1.y;
						} else {
							move += move2.y;
						}
							
						// 當兩指距離越遠，Z位置加的越多，相反之
						Camera.main.transform.Translate(0, 0, move * Time.deltaTime);
					}
				}
			}
		}
	}

	private gDefine.Direction HandDirection(Vector2 StartPos, Vector2 EndPos) {
		if (Vector2.Distance(StartPos, EndPos) < 10) {
			return gDefine.Direction.Click;
		}

		gDefine.Direction direction;

		//手指水平移動
		if (Mathf.Abs(StartPos.x - EndPos.x) > Mathf.Abs(StartPos.y - EndPos.y)) {
			if (StartPos.x > EndPos.x) {
				direction = gDefine.Direction.Left; //手指向左滑動
			} else {
				direction = gDefine.Direction.Right; //手指向右滑動
			}
		} else {
			if (screenPos.y > EndPos.y) {
				direction = gDefine.Direction.Down; //手指向下滑動
			} else {
				direction = gDefine.Direction.Up; //手指向上滑動
			}
		}

		Answer((int)direction);

		return direction;
	}

	protected override void CreateQuestion() {
		base.CreateQuestion();

		reaction = "";
		
		directionImages[currentDirection].gameObject.SetActive(false);
		currentDirection = rand.Next(4);
		currentColor = rand.Next(2);
		directionImages[currentDirection].gameObject.SetActive(true);
		directionImages[currentDirection].color = colors[currentColor];

		Utils.Instance.PlayAnimation(directionImages[currentDirection].GetComponent<Animation>(), "card_fadein");

		type = typeCodes[currentColor];
		question = questionCodes[currentDirection];
	}

	private void Answer(int direction) {
		reaction += questionCodes[direction];

		if (currentColor == 1) {
			direction += (direction % 2 == 0) ? 1 : -1;
		}

		if (currentDirection == direction) {
			Game.self.Right();
			SaveQuestion();
			CreateQuestion();
		} else {
			Game.self.Wrong();
		}
	}

	public override void GameOver() {
		Game.self.Next(true, Game.self.GetReactionCount() > 0);
	}
}