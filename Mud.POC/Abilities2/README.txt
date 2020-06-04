AbilityInfo
	string Name
	AbilityEffects Effects
    int PulseWaitTime
    int Cooldown
    int LearnDifficultyMultiplier
	AbilityAdditionalInfoAttribute[] AdditionalAttributes
		can be of type
		AbilityCharacterWearOffMessageAttribute
		AbilityItemWearOffMessageAttribute
		AbilityDispellableAttribute
		AbilityDamageNounAttribute
	Type AbilityType -> used to create ability instance must be inhered from GameAction

AbilityManager
	List<AbilityInfo> AbilityInfos

AbilityUsage // used by race/class to list own abilities
	string Name
	int Level
	ResourceKinds? ResourceKind
	int CostAmount
	CostAmountOperators CostAmountOperator
	int Rating

AbilityLearned : AbilityUsage // used by character to list own abilities
	int Learned

ActionInput
	IEntity Actor
	string command
	string RawParameters
	CommandParameters[] Parameters

AbilityActionInput: ActionInput
	AbilityInfo

IGameAction<T>
	where T: ActionInput
	Guards(T) // will use ActionInput to get target/... and check if action if executable
	Execute(T) // will execute action

IAbilityAction : IGameAction<AbilityActionInput>

TODO:
	Register every spell in DependencyContainer
	Unit tests
	GameAction to cast a spell -> will search spell and trim arguments to create an AbilityActionInput
	SkillBase