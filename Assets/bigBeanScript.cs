using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class bigBeanScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable Bean;
	public GameObject Text;
	public KMBombModule Module;

	private int bean = 0;
	private float offset;
	private int timeoffset;
	private int[][] colours = new int[][] { new int[] { 192, 192, 0 }, new int[] { 84, 144, 192 }, new int[] { 0, 0, 0 } };
	private int eatensteps = 0;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed()
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Monch", Module.transform);
			//Beans[pos].GetComponent<Renderer>().enabled = false;
			switch (eatensteps)
            {
				case 0:
					Bean.transform.localScale = new Vector3(0.075f, 0.075f, 0.12f);
					break;
				case 1:
					Bean.transform.localScale = new Vector3(0.05f, 0.05f, 0.08f);
					break;
				case 2:
					Bean.transform.localScale = new Vector3(0f, 0f, 0f);
					break;
			}
			eatensteps++;
			return false;
		};
	}

	private void BeanHovered()
	{
		string[] colour = { "orange", "yellow", "green" };
		Text.GetComponent<TextMesh>().text = colour[bean % 3];
	}

	private void BeanHoverEnded()
	{
		Text.GetComponent<TextMesh>().text = "";
	}

	void Awake()
	{
		_moduleID = _moduleIdCounter++;

		Bean.OnInteract += BeanPressed();
		Bean.OnHighlight += delegate { BeanHovered(); return; };
		Bean.OnHighlightEnded += delegate { BeanHoverEnded(); return; };
	}

	void Start () {

		offset = Rnd.Range(0f, 360f);
		timeoffset = Rnd.Range(0, 100);
		bean = Rnd.Range(0, 6);
		bool[] initvalid = { false, false, true, false, true, false };
		Bean.GetComponent<MeshRenderer>().material.color = new Color(colours[0][bean % 3] / 255f, colours[1][bean % 3] / 255f, colours[2][bean % 3] / 255f);
		StartCoroutine(Wobble());
	}

	private IEnumerator Wobble()
	{
		float t = 0;
		while (true)
		{
			if (bean / 3 == 1)
			{
				Bean.transform.localEulerAngles = new Vector3(0f, Mathf.Sin(t + timeoffset) * 15f + offset, 0f);
			}
			else
			{
				Bean.transform.localEulerAngles = new Vector3(0f, offset, 0f);
			}
			t += 0.125f;
			yield return new WaitForSeconds(0.02f);
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} highlight' to highlight the bean, '!{0} 2' to eat the bean twice. Note that you cannot press it more than 3 times.";
#pragma warning restore 414
	//IEnumerator ProcessTwitchCommand(string command)
	//{
	//	yield return null;
	//	command = command.ToLowerInvariant();
	//	if (command == "cycle")
	//	{
    //        for (int i = 0; i < 9; i++)
    //        {
    //            while (i < 9 && Beans[i].transform.localScale.x < 0.01f)
    //            {
	//				i++;
    //            }
    //            if (i != 9)
    //            {
	//				Beans[i].OnHighlight();
	//				yield return new WaitForSeconds(0.9f);
	//				Beans[i].OnHighlightEnded();
	//				yield return new WaitForSeconds(0.1f);
	//			}
    //        }
	//	}
	//	else
	//	{
	//		string validCommands = "123456789";
	//		command = command.Replace(" ", "");
	//		for (int i = 0; i < command.Length; i++)
	//		{
	//			if (!validCommands.Contains(command[i]))
	//			{
	//				yield return "sendtochaterror Invalid command.";
	//				yield break;
	//			}
	//		}
	//		for (int i = 0; eatenbeans != 3 && i < command.Length; i++)
	//		{
	//			yield return null;
	//			for (int j = 0; j < validCommands.Length; j++)
	//			{
	//				if (command[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.01f) { Beans[j].OnInteract(); }
	//			}
	//			yield return new WaitForSeconds(0.5f);
	//		}
	//		yield return "strike";
	//		yield return "solve";
	//	}
	//	yield return null;
	//}
	//
	//IEnumerator TwitchHandleForcedSolve()
	//{
	//	yield return null;
	//	for (int i = 0; i < 9 && eatenbeans != 3; i++)
	//	{
	//		if (beansafe[i])
	//		{
	//			Beans[i].OnInteract();
	//			yield return new WaitForSeconds(0.5f);
	//		}
	//	}
	//	for (int i = 0; i < 9 && eatenbeans != 3; i++)
	//	{
    //        if (Beans[i].transform.localScale.x > 0.01f)
    //        {
	//			Beans[i].OnInteract();
	//			yield return new WaitForSeconds(0.5f);
	//		}
	//	}
	//}
}
