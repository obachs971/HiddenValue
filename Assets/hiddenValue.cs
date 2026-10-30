using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;
using System.Text.RegularExpressions;

public class hiddenValue : MonoBehaviour {

	// Use this for initialization
	public KMBombInfo bomb;
	public new KMAudio audio;
	public KMBombModule module;
	public MeshRenderer back;
	public TextMesh text;
	public Material[] mats;
	public MeshRenderer[] leds;
	public KMSelectable[] hinges;
	public MeshRenderer[] hingeMesh;
	public MeshRenderer modulePlate;
	public AudioClip solveSound;
	static int moduleIdCounter = 1;
	int moduleId;

	private List<int[]> positions = new List<int[]>()
	{
		new int[]{ 50, 50 },
		new int[]{ 50, 90 },
		new int[]{ 50, 130 },

		new int[]{ 90, 50 },

		new int[]{ 90, 130 },

		new int[]{ 130, 50 },
		new int[]{ 130, 90 },
		new int[]{ 130, 130 }
	};
	private string[][] chart =
	{
		new string[7]{"9", "45", "8", "5", "6", "6", "0"},
		new string[7]{"3", "7", "25", "4", "6", "1", "59"},
		new string[7]{"2", "2", "37", "47", "0", "8", "9"},
		new string[7]{"4", "0", "3", "6", "13", "4", "3"},
		new string[7]{"16", "4", "7", "1", "4", "9", "08"},
		new string[7]{"5", "0", "9", "8", "34", "1", "6"},
		new string[7]{"68", "34", "7", "5", "5", "3", "2"}
	};
	private List<int> numbers = new List<int>();
	private List<char> numberColors = new List<char>();
	private List<Material> numberMat = new List<Material>();
	private List<int[]> orientations = new List<int[]>();
	private int flips;
	private string[] hingeTimes = {"", "", "", "", "", "", "", "", ""};
	private bool moduleSolved;
	void Awake () 
	{
		moduleId = moduleIdCounter++;
		hinges[0].OnInteract += delegate () { pressed(0); return false; };
		hinges[1].OnInteract += delegate () { pressed(1); return false; };
		hinges[2].OnInteract += delegate () { pressed(2); return false; };
		hinges[3].OnInteract += delegate () { pressed(3); return false; };
		hinges[4].OnInteract += delegate () { pressed(4); return false; };
		hinges[5].OnInteract += delegate () { pressed(5); return false; };
		hinges[6].OnInteract += delegate () { pressed(6); return false; };
		hinges[7].OnInteract += delegate () { pressed(7); return false; };
	}
	void Start()
    {
		moduleSolved = false;
		flips = -1;
		var numValues = UnityEngine.Random.Range(0, 3) + 4;
		string numChoice = new string("123456789".ToCharArray().Shuffle());
		string sn = bomb.GetSerialNumber();
		string snnums = "";
		for(int aa = 0; aa < sn.Length; aa++)
        {
			if ("0123456789".IndexOf(sn[aa]) >= 0)
				snnums = snnums + "" + sn[aa];
        }
		for (int aa = 0; aa < numValues; aa++)
        {
			numbers.Add(numChoice[aa] - '0');
			numberColors.Add("RGWYMCP"[UnityEngine.Random.Range(0, 7)]);
			numberMat.Add(mats["RGWYMCP".IndexOf(numberColors[aa])]);
			orientations.Add(new int[4]);
			int num = UnityEngine.Random.Range(0, positions.Count);
			orientations[aa][0] = positions[num][0] - 19;
			orientations[aa][1] = positions[num][0] + 19;
			orientations[aa][2] = positions[num][1] - 19;
			orientations[aa][3] = positions[num][1] + 19;
			Debug.LogFormat("[The Hidden Value #{0}] Orientation #{1}: {2}, {3}", moduleId, aa + 1, positions[num][0], positions[num][1]);
			positions.RemoveAt(num);
			if (numbers[aa] == ((snnums[1] - '0') - 1))
			{
				if (UnityEngine.Random.Range(0, 2) == 0)
					numbers[aa] = 0;
			}
			Debug.LogFormat("[The Hidden Value #{0}]: Digit #{1}: {2}{3}", moduleId, aa + 1, numberColors[aa], numbers[aa]);
			if (numbers[aa] == 1 || numbers[aa] == 7)
				hingeTimes[numbers[aa] - 1] = hingeTimes[numbers[aa] - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][0];
			else if (numbers[aa] == 2 || numbers[aa] == 4)
				hingeTimes[numbers[aa] - 1] = hingeTimes[numbers[aa] - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][1];
			else if (numbers[aa] == 5 || numbers[aa] == 8)
				hingeTimes[numbers[aa] - 1] = hingeTimes[numbers[aa] - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][2];
			else if (numbers[aa] == 3)
				hingeTimes[numbers[aa] - 1] = hingeTimes[numbers[aa] - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][3];
			else if (numbers[aa] == 6)
				hingeTimes[numbers[aa] - 1] = hingeTimes[numbers[aa] - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][4];
			else if (numbers[aa] == 9)
				hingeTimes[numbers[aa] - 1] = hingeTimes[numbers[aa] - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][5];
			else
				hingeTimes[(snnums[1] - '0') - 2] = hingeTimes[(snnums[1] - '0') - 1] + "" + chart["RGWYMCP".IndexOf(numberColors[aa])][6];
		}
		for(int aa = 0; aa < hingeTimes.Length; aa++)
        {
			if(hingeTimes[aa].Length > 0)
            {
				Debug.LogFormat("[The Hidden Value #{0}] Hinge #{1} time: {2}", moduleId, aa + 1, hingeTimes[aa]);
			}
        }
	}
	// Update is called once per frame
	void Update () 
	{
		if(!moduleSolved)
        {
			int cur = -1;
			var angle = Vector3.Angle(back.transform.up, Camera.main.transform.up);
			var angle2 = Vector3.Angle(back.transform.up, Camera.main.transform.right);
			//Debug.LogFormat("[The Hidden Value #{0}] Angle {1}, {2}", moduleId, angle, angle2);
			for (int aa = 0; aa < numbers.Count; aa++)
			{
				if ((int)(angle) >= orientations[aa][0] && (int)(angle) <= orientations[aa][1] && (int)(angle2) >= orientations[aa][2] && (int)(angle2) <= orientations[aa][3])
				{
					cur = aa;
					break;
				}
			}
			//text.text = (int)(Vector3.Angle(back.transform.up, Camera.main.transform.up)) + ", " + (int)(Vector3.Angle(back.transform.up, Camera.main.transform.right));
			if (cur != flips)
			{
				for (int aa = 0; aa < 7; aa++)
					leds[aa].material = mats[7];
				if (cur >= 0)
				{
					switch (numbers[cur])
					{
						case 0:
							leds[0].material = numberMat[cur];
							leds[1].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[4].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
						case 1:
							leds[2].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							break;
						case 2:
							leds[0].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[4].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
						case 3:
							leds[0].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
						case 4:
							leds[1].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							break;
						case 5:
							leds[0].material = numberMat[cur];
							leds[1].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
						case 6:
							leds[0].material = numberMat[cur];
							leds[1].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[4].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
						case 7:
							leds[0].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							break;
						case 8:
							leds[0].material = numberMat[cur];
							leds[1].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[4].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
						case 9:
							leds[0].material = numberMat[cur];
							leds[1].material = numberMat[cur];
							leds[2].material = numberMat[cur];
							leds[3].material = numberMat[cur];
							leds[5].material = numberMat[cur];
							leds[6].material = numberMat[cur];
							break;
					}
				}
				flips = cur;
			}
		}
	}
	void pressed(int hn)
    {
		if(!moduleSolved)
        {
			int lastSecond = (int)(bomb.GetTime() % 10);
			hinges[hn].AddInteractionPunch();
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			var num = -1;
			if (hingeTimes[hn].Equals("") && !(hingeTimes[8].Equals("")) && hingeTimes[8].Contains(lastSecond + ""))
					num = 9;
			else if (hingeTimes[hn].Contains(lastSecond + ""))
				num = hn + 1;
			
			if (num == -1)
			{
				//Strike
				Debug.LogFormat("[The Hidden Value #{0}] Incorrect! You pressed hinge #{1} at {2}", moduleId, hn + 1, lastSecond);
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				module.HandleStrike();
			}
			else
			{
				//Press
				hingeMesh[hn].transform.localScale = new Vector3(0, 0, 0);
				hingeTimes[num - 1] = "";
				num = numbers.IndexOf(num);
				if (num == -1)
					num = numbers.IndexOf(0);
				numbers.RemoveAt(num);
				numberColors.RemoveAt(num);
				numberMat.RemoveAt(num);
				orientations.RemoveAt(num);
			}
			if (numbers.Count == 0)
			{
				//Solved
				for (int aa = 0; aa < 7; aa++)
					leds[aa].material = mats[7];
				moduleSolved = true;
				StartCoroutine(animation());
			}
		}
	}
	
	IEnumerator animation()
    {
		modulePlate.transform.localRotation = new Quaternion(0, 0, 0, -1f);
		modulePlate.transform.localPosition = new Vector3(modulePlate.transform.localPosition.x, 0.1f, modulePlate.transform.localPosition.z);
		float rotatex = 0;
		float rotatey = 0;
		float rotatez = 0;
		float rotatew = -1f;
		float positionx = modulePlate.transform.localPosition.x;
		float positiony = modulePlate.transform.localPosition.y;
		float positionz = modulePlate.transform.localPosition.z;
		for(int aa = 0; aa < 100; aa++)
		{
			rotatez += 0.01f;
			positiony += 0.0001f;
			positionx += 0.0008f;
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			yield return new WaitForSeconds(0.01f);
		}
		yield return new WaitForSeconds(1f);
		for(int aa = 0; aa < 100; aa++)
		{
			rotatex += 0.005f;
			rotatey += 0.005f;
			positionz -= 0.0008f;
			positiony += 0.00025f;
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			yield return new WaitForSeconds(0.005f);
		}
		for (int aa = 0; aa < 50; aa++)
		{
			rotatex -= 0.005f;
			rotatey -= 0.005f;
			positionz += 0.0008f;
			positiony -= 0.00025f;
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			yield return new WaitForSeconds(0.005f);
		}
		for (int aa = 0; aa < 50; aa++)
		{
			rotatex += 0.005f;
			rotatey += 0.005f;
			positionz -= 0.0008f;
			positiony += 0.00025f;
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			yield return new WaitForSeconds(0.005f);
		}
		for (int aa = 0; aa < 50; aa++)
		{
			rotatex -= 0.005f;
			rotatey -= 0.005f;
			positionz += 0.0008f;
			positiony -= 0.00025f;
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			yield return new WaitForSeconds(0.005f);
		}
		for (int aa = 0; aa < 50; aa++)
		{
			rotatex += 0.005f;
			rotatey += 0.005f;
			positionz -= 0.0008f;
			positiony += 0.00025f;
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			yield return new WaitForSeconds(0.005f);
		}
		for (int aa = 0; aa < 100; aa++)
		{
			positionz -= 0.02f;
			rotatew += 0.02f;
			modulePlate.transform.localPosition = new Vector3(positionx, positiony, positionz);
			modulePlate.transform.localRotation = new Quaternion(rotatex, rotatey, rotatez, rotatew);
			yield return new WaitForSeconds(0.005f);
		}
		modulePlate.transform.localScale = new Vector3(0f, 0f, 0f);
		audio.PlaySoundAtTransform(solveSound.name, transform);
		module.HandlePass();
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} cycle cycles through all the numbers. !{0} press 2 at 4 presses the 2nd hinge when the last digit in the countdown timer is a 4.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] param = command.Split(' ');
		bool flag = true;
		if(param.Length == 4)
		{
			if (Regex.IsMatch(param[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && Regex.IsMatch(param[2], @"^\s*at\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && param.Length == 4)
			{
				if ("12345678".IndexOf(param[1]) >= 0 && param[1].Length == 1 && "0123456789".IndexOf(param[3]) >= 0 && param[3].Length == 1)
				{
					flag = false;
					int timepress = "0123456789".IndexOf(param[3]);
					yield return null;
					while (((int)(bomb.GetTime())) % 10 != timepress)
						yield return "trycancel The button was not pressed due to a request to cancel.";
					yield return "strike";
					yield return "solve";
					hinges["12345678".IndexOf(param[1])].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		
		if (Regex.IsMatch(param[0], @"^\s*cycle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			flag = false;
			for (int aa = 0; aa < numbers.Count; aa++)
            {
				switch (numbers[aa])
				{
					case 0:
						leds[0].material = numberMat[aa];
						leds[1].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[4].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
					case 1:
						leds[2].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						break;
					case 2:
						leds[0].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[4].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
					case 3:
						leds[0].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
					case 4:
						leds[1].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						break;
					case 5:
						leds[0].material = numberMat[aa];
						leds[1].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
					case 6:
						leds[0].material = numberMat[aa];
						leds[1].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[4].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
					case 7:
						leds[0].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						break;
					case 8:
						leds[0].material = numberMat[aa];
						leds[1].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[4].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
					case 9:
						leds[0].material = numberMat[aa];
						leds[1].material = numberMat[aa];
						leds[2].material = numberMat[aa];
						leds[3].material = numberMat[aa];
						leds[5].material = numberMat[aa];
						leds[6].material = numberMat[aa];
						break;
				}
				yield return new WaitForSeconds(1.5f);
				for(int bb = 0; bb < leds.Length; bb++)
                {
					leds[bb].material = mats[7];
                }
				yield return new WaitForSeconds(0.5f);
			}
		}
		if (flag)
			yield return "sendtochat The command you sent wasn't executed because the hinges were confused/scared.";
	}
}
