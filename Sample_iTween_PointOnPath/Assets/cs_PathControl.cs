/*
 *	@file	cs_PathControl
 *	@note		なし 
 *	@attention	
 *				[cs_PathControl.cs]
 *				Copyright (c) [2015] [Maruton]
 *				This software is released under the MIT License.
 *				http://opensource.org/licenses/mit-license.php
 */

using UnityEngine;
using System.Collections;

public class cs_PathControl : MonoBehaviour {
	Vector3[] NodeData;
	iTweenPath cp_ITweenPath;	//!< Instance pointer for 'ITween Path' Script
	float PositionPercent = 0; //!< 0.00~1.00. パス全体の移動距離に対し％で指定する値 
	
	/*!
	 *	Visual Path Editorで作成済みのパスデータを取得
	 * 	@param[in]	pathname	取得するパス名
	 * 	@return					取得結果  true:成功  false:失敗
	 * 	@note					Visual Path Editorで作成済みのパスデータ Node 1~n を NodeData[]へ格納する 
	 * 	@attention				iTweenPath.GetPath()内部のパス名リスト更新が OnEnable()のタイミングで行われている。 
	 * 							よってAwakeではまだ確定していない場合がありえる為、Start()以降のタイミングで呼び出すこと。 
	 */
	bool SetPathPosition_from_ITweenPath(string pathname){
		if(cp_ITweenPath==null){
			cp_ITweenPath = GetComponent<iTweenPath>(); // このObject内の ITween path スクリプトコンポーネントのインスタンスを取得 
			if(cp_ITweenPath==null){
				Debug.Log("[NO SCRIPT] No there script 'ITween Path' on this object");
				return(false);
			}
		}
		// GetPath()はStatic宣言されているので呼び出し可  
		NodeData = iTweenPath.GetPath(pathname); // Visual Path Editor のパス名を指定してパスデータを取得する。 
		if(NodeData==null){
			Debug.Log ("[NOT FOUND/null] path name '"+pathname+"'");
			return(false);
		}
		return(true);
	}
	const float addSpeed = 0.005f;
	void Drive_Increase(){
		PositionPercent += addSpeed;
		if(PositionPercent>1.0f) PositionPercent = 1.0f;
	}
	void Drive_Decrease(){
		PositionPercent -= addSpeed;
		if(PositionPercent<0.0f) PositionPercent = 0.0f;
	}
	
	void Watch_UI_Input(){
		bool drive_Fwd = false;
		bool drive_Bwd = false;
		
		//Begin: Control by Keyboard
		bool input_Key_A = Input.GetKey(KeyCode.A);
		bool input_Key_D = Input.GetKey(KeyCode.D);
		if( !(input_Key_A & input_Key_D) ){ // Disable multi press
			drive_Fwd |= input_Key_A;
			drive_Bwd |= input_Key_D;
		}
		//End: Control by Keyboard
		#if UNITY_ANDROID
		//Begin: Control by Accel Sensor
		float accel_X = Input.acceleration.x; // 加速度センサ 横傾き 
		//Debug.Log ("accel_X:"+accel_X);
		if(accel_X <-0.2f){
			drive_Fwd |= true;
		}
		else if(accel_X >0.2f){
			drive_Bwd = true;
		}
		//End: Control by Accel Sensor
		#endif
		
		if (drive_Fwd) Drive_Increase();
		if (drive_Bwd) Drive_Decrease();
		if(drive_Fwd|drive_Bwd){
			//Procedure when moving action.
		}
	}
	
	void Start () {
		SetFontStyle_for_OnGUI();//Initial display for OnGUI().
		SetPathPosition_from_ITweenPath("MyPath");//Setup iTween path data.
	}
	
	void Update () {
		Watch_UI_Input();
		transform.position = iTween.PointOnPath(NodeData, PositionPercent); // Get target position 
	}


	GUIStyle labelStyle;	//!< GUIフォント表示用スタイル
	void SetFontStyle_for_OnGUI(){
		const int fontRetio = 20;//!< Font size retio.
		labelStyle = new GUIStyle();
		labelStyle.fontSize = Screen.width / fontRetio; // Calc font size
		labelStyle.normal.textColor = Color.white;
		labelStyle.wordWrap = true;
	}
	void OnGUI(){
		string text;
		text = Screen.width+"x"+Screen.height;
		text += "\nPC=[A][D]Key\nAndroid=Tilt Left/Right";
		text += "\nPos "+PositionPercent.ToString("0.00%");
		GUI.Label(new Rect(0, 0, Screen.width-100, Screen.height-100), text, labelStyle);
	}
}
