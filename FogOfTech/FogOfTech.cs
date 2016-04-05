using System;
using UnityEngine;
using KSP.UI.Screens;

/// <summary>
/// FogOfTech
/// hides the nodes of the KSP TechTree that you haven't researched preceding techs for
/// Licensed under MIT license
/// </summary>
namespace FogOfTech
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class FogOfTech : MonoBehaviour
	{
		
		public static String AppName = "FogOfTech";
		
		private static bool DEBUG = false;
		
		private static ParentSetting parentTechsToUnlock = ParentSetting.DEFAULT;
		
		private static RDTechTree currentTechTree = null;
		
		public enum ParentSetting {
			ANY,
			ALL,
			DEFAULT
		}
		
		public FogOfTech()
		{
		}
		
		void onTechTreeSpawn(RDTechTree techTree) {
			printDebug("Tree spawned");
			currentTechTree = techTree;
			updateTree(techTree);
		}
		
		void onTechTreeDespawn(RDTechTree techTree) {
			printDebug("Tree despawned");
			currentTechTree = null;
		}
		
		void onTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> targetAction) {
			if (RDTech.OperationResult.Successful.Equals(targetAction.target) && (currentTechTree != null)) {
				updateTree(currentTechTree);
				//currentTechTree.SpawnTechTreeNodes();
			}
		}
		
		private void updateTree(RDTechTree techTree) {
			if (techTree != null) {
				foreach (RDNode node in techTree.controller.nodes) {
					if (ResearchAndDevelopment.GetTechnologyState(node.tech.techID) != RDTech.State.Available) {
						updateNode(node);
					}
				}
			}
		}
		
		private void updateNode(RDNode node) {
			bool allParentsResearched = true;
			bool anyParentResearched = false;
			foreach (RDNode.Parent parentNode in node.parents) {
				if (parentNode.parent == null) {
					continue; //this happens with improper ModuleManager created trees. Just skip processing of that buggy parent
				}
				bool isResearched = ResearchAndDevelopment.GetTechnologyState(parentNode.parent.node.tech.techID) == RDTech.State.Available;
				allParentsResearched &= isResearched;
				anyParentResearched |= isResearched;
			}
			printDebug("parentTechsToShow=" + parentTechsToUnlock+ " node.AnyParentToUnlock=" + node.AnyParentToUnlock + " allParentsResearched=" + allParentsResearched + " anyParentResearched=" + anyParentResearched);
			if ((ParentSetting.ALL.Equals(parentTechsToUnlock) || ParentSetting.DEFAULT.Equals(parentTechsToUnlock) && !node.AnyParentToUnlock) && !allParentsResearched ||
			    (ParentSetting.ANY.Equals(parentTechsToUnlock) || ParentSetting.DEFAULT.Equals(parentTechsToUnlock) && node.AnyParentToUnlock) && (node.parents.Length > 0) && !anyParentResearched) {
				node.gameObject.SetActive(false);
			} else {
				node.gameObject.SetActive(true);
			}
		}
		
		private void loadConfig() {
			foreach (ConfigNode configNode in GameDatabase.Instance.GetConfigNodes("FogOfTech")) {
				parentTechsToUnlock = parseParentSetting(configNode.GetValue("parentTechsToShow"));
				printDebug("loadConfig " + parentTechsToUnlock);
			}
		}
		
		private ParentSetting parseParentSetting(String str) {
			if (Enum.IsDefined(typeof(ParentSetting), str)) {
				return (ParentSetting)Enum.Parse(typeof(ParentSetting), str);
			} else {
				return ParentSetting.DEFAULT;
			}
		}
		
		/*
		 * Called after the scene is loaded.
		 */
		void Awake()
		{
			printDebug("Awake");
			loadConfig();
			RDTechTree.OnTechTreeSpawn.Add(new EventData<RDTechTree>.OnEvent(onTechTreeSpawn));
			RDTechTree.OnTechTreeDespawn.Add(new EventData<RDTechTree>.OnEvent(onTechTreeSpawn));
			GameEvents.OnTechnologyResearched.Add(new EventData<GameEvents.HostTargetAction<RDTech, RDTech.OperationResult>>.OnEvent(onTechnologyResearched));
		}
		
		/*
		 * Called next.
		 */
		void Start()
		{
			printDebug( " start");
			DontDestroyOnLoad(this);
		}

		/*
		 * Called every frame
		 */
		void Update()
		{
			
		}

		/*
		 * Called at a fixed time interval determined by the physics time step.
		 */
		void FixedUpdate()
		{
			
		}

		/*
		 * Called when the game is leaving the scene (or exiting). Perform any clean up work here.
		 */
		void OnDestroy()
		{
			printDebug(" destroy");
		}
		
		public static void printDebug(String str) {
			if (DEBUG) {
				print(AppName.ToUpper() + ":: " + str);
			}
		}
		
	}
}