﻿Feature: Equipment

Background: 
	Given Есть карта размером 2
	And Есть актёр игрока класса human-person в ячейке (0, 0)

@dev1 @equipment
Scenario Outline: Снятие экипированного предмета из слота.
	Given Актёр игрока экипирован предметом steel-helmet в слот Index: 0
	And Актёр игрока экипирован предметом steel-armor в слот Index: 1
	And Актёр игрока экипирован предметом battle-axe в слот Index: 2
	And Актёр игрока экипирован предметом short-sword в слот Index: 3
	When Снимаю экипировку из слота <slotIndex>
	And Жду 1000 единиц времени
	Then В слоте Index: <slotIndex> актёра игрока ничего нет
	And У актёра в инвентаре есть <propSid>

	Examples: 
	| slotIndex | propSid      |
	| 0         | steel-helmet |
	| 1         | steel-armor  |
	| 2         | battle-axe   |
	| 3         | short-sword  |

@equipment @dev1 @dev18
Scenario Outline: Замена одного примета другим.
	Given В инвентаре у актёра игрока есть предмет: <propSid1>
	And В инвентаре у актёра игрока есть предмет: <propSid2>
	When Экипирую предмет <propSid1> в слот Index: <slotIndex>
	And Жду 1000 единиц времени
	And Экипирую предмет <propSid2> в слот Index: <slotIndex>
	And Жду 1000 единиц времени
	Then В слоте Index: <slotIndex> актёра игрока есть <propSid2>
	And У актёра в инвентаре есть <propSid1>

	Examples: 
	| propSid1    | propSid2    | slotIndex |
	| short-sword | short-sword | 2         |
	| pistol      | short-sword | 2         |
	| short-sword | pistol      | 2         |
	| short-sword | great-sword | 2         |

@equipment @dev1
Scenario Outline: Замена одного примета другим. Работа с щитами.
	Given Актёр игрока экипирован предметом <propSid1> в слот Index: <slotIndex1>
	And Актёр игрока экипирован предметом <propSid2> в слот Index: <slotIndex2>
	And В инвентаре у актёра игрока есть предмет: <propSid3>
	When Экипирую предмет <propSid3> в слот Index: <slotIndex3>
	And Жду 1000 единиц времени
	Then В слоте Index: <slotIndex3> актёра игрока есть <propSid3>
	And У актёра в инвентаре есть <propSid2>

	Examples: 
	| propSid1    | slotIndex1 | propSid2    | slotIndex2 | propSid3      | slotIndex3 |
	| short-sword | 2          | short-sword | 3          | wooden-shield | 3          |
	| short-sword | 2          | short-sword | 3          | wooden-shield | 2          |

@equipment @dev18
Scenario Outline: Двуручное оружие вытесняет текущую экипировку в инвентарь.
	Given Актёр игрока экипирован предметом <propSid1> в слот Index: <slotHandIndex1>
	And Актёр игрока экипирован предметом <propSid2> в слот Index: <slotHandIndex2>
	And В инвентаре у актёра игрока есть предмет: <propSid3>
	When Экипирую предмет <propSid3> в слот Index: <targetSlotIndex>
	And Жду 1000 единиц времени
	Then В слоте Index: <targetSlotIndex> актёра игрока есть <propSid3>
	And У актёра в инвентаре есть <propSid1>
	And У актёра в инвентаре есть <propSid2>

	Examples: 
	| propSid1    | slotHandIndex1 | propSid2      | slotHandIndex2 | propSid3     | targetSlotIndex |
	# Щит и меч. Оба должны быть вытеснены в инвентарь двуручным большим мечом.
	| short-sword | 2              | wooden-shield | 3              | great-sword  | 2               |
	# Один двуручник успешно замечает другой.
	| great-sword | 2              | нет           | 2              | combat-staff | 2               |
	# Один двуручник успешно замечает другой даже если второй взяли в другую руку.
	| great-sword | 2              | нет           | 2              | combat-staff | 3               |

@equipment @dev1
Scenario Outline: Экипировка предмета в подходящий слот.
	Given В инвентаре у актёра игрока есть предмет: <propSid>
	When Экипирую предмет <propSid> в слот Index: <slotIndex>
	And Жду 1000 единиц времени
	Then В слоте Index: <testedSlotIndex> актёра игрока есть <propSid>
	And Параметр <paramType> равен <paramValue>

	Examples: 
	| propSid     | slotIndex | testedSlotIndex | paramType | paramValue |
	| short-sword | 2         | 2               | -         | 0          |

@equipment @dev1
Scenario Outline: Экипировка двух пистолетов не разрешена.
	Given В инвентаре у актёра игрока есть предмет: <propSid1>
	And В инвентаре у актёра игрока есть предмет: <propSid2>
	When Экипирую предмет <propSid1> в слот Index: <slotIndex1>
	And Жду 1000 единиц времени
	Then В слоте Index: <slotIndex1> актёра игрока есть <propSid1>
	And Невозможна экипировка предмета <propSid2> в слот Index: <slotIndex2>

	Examples: 
	| propSid1    | slotIndex1 | propSid2 | slotIndex2 |
	| pistol      | 2          | pistol   | 3          |
	| short-sword | 2          | pistol   | 3          |

@equipment @dev1
Scenario Outline: Экипировка двух щитов не разрешена.
	Given В инвентаре у актёра игрока есть предмет: <propSid1>
	And В инвентаре у актёра игрока есть предмет: <propSid2>
	When Экипирую предмет <propSid1> в слот Index: <slotIndex1>
	And Жду 1000 единиц времени
	Then В слоте Index: <slotIndex1> актёра игрока есть <propSid1>
	And Невозможна экипировка предмета <propSid2> в слот Index: <slotIndex2>

	Examples: 
	| propSid1      | slotIndex1 | propSid2      | slotIndex2 |
	| wooden-shield | 2          | wooden-shield | 3          |

@equipment @dev1
Scenario Outline: Экипировка двух оружий.
	Given В инвентаре у актёра игрока есть предмет: <propSid1>
	And В инвентаре у актёра игрока есть предмет: <propSid2>
	When Экипирую предмет <propSid1> в слот Index: <slotIndex1>
	And Жду 1000 единиц времени
	And Экипирую предмет <propSid2> в слот Index: <slotIndex2>
	And Жду 1000 единиц времени
	Then В слоте Index: <slotIndex1> актёра игрока есть <propSid1>
	And В слоте Index: <slotIndex2> актёра игрока есть <propSid2>

	Examples: 
	| propSid1    | slotIndex1 | propSid2    | slotIndex2 |
	| short-sword | 2          | short-sword | 3          |

@props @equipment @chests @dev1
Scenario: Предметы из инвентаря можно экипировать.
	Given Есть сундук Id:500 в ячейке (0, 1)
	And Сундук содержит Id:500 экипировку pistol
	When Я выбираю сундук Id:500
	And Я забираю из сундука экипировку pistol
	And Жду 1000 единиц времени
	And Экипирую предмет pistol в слот Index: 2
	And Жду 1000 единиц времени
	Then В слоте Index: 2 актёра игрока есть pistol

@props @equipment @dev2
Scenario: Предметы дают бонус к здоровью.
	Given В инвентаре у актёра игрока есть предмет: highlander-helmet
	When Экипирую предмет highlander-helmet в слот Index: 0
	And Жду 1000 единиц времени
	Then Текущий запас здоровья персонажа игрока равен 13
	And Максимальный запас здоровья персонажа игрока равен 13

@props @equipment @dev2
Scenario: Правило BonusIfNoChest, если есть броня
	Given В инвентаре у актёра игрока есть предмет: highlander-helmet
	And Актёр игрока экипирован предметом steel-armor в слот Index: 1
	When Экипирую предмет highlander-helmet в слот Index: 0
	Then Текущий запас здоровья персонажа игрока равен 10
	And Максимальный запас здоровья персонажа игрока равен 10