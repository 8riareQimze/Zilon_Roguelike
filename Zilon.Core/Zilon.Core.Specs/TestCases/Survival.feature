﻿Feature: Survival
	Чтобы ввести микроменеджмент ресурсов и состояния персонажей
	Как игроку
	Мне нужно, чтобы при употреблении еды повышалась сытость персонажа
	Чтобы эмулировать восстановление сил персонажа при угрозах выживания
	Как разработчику
	Мне нужно, чтобы при употреблении провинта разного типа (еда/вода)
	сбрасывались соответствующие угрозы выживания при насыщении персонажа

#@survival @dev1
#Scenario Outline: Восстановление параметров провиантом
#	Given Есть карта размером 2
#	And Есть актёр игрока класса human-person в ячейке (0, 0)
#	And В инвентаре у актёра есть еда: <propSid> количество: 1
#	When Актёр использует предмет <propSid> на себя
#	Then Значение <stat> повысилось на <propValue> и уменьшилось на 1 за игровой цикл и стало <expectedStatValue>
#	And Предмет <propSid> отсутствует в инвентаре актёра
#
#
#Examples: 
#	| propSid      | stat    | propValue | expectedStatValue |
#	| kalin-cheese | сытость | 70        | 220               |
#	| water-bottle | вода    | 150       | 299               |

#@survival @dev1 @dev2
#Scenario Outline: Снятие угроз выживания
#	Given Есть карта размером 2
#	And Есть актёр игрока класса human-person в ячейке (0, 0)
#	And Актёр значение <stat> равное <statValue>
#	And В инвентаре у актёра есть фейковый провиант <propSid> (<provisionStat>)
#	When Актёр использует предмет <propSid> на себя
#	Then Актёр под эффектом <effect>
#
#Examples: 
#| stat    | statValue | propSid    | provisionStat | propCount | expectedValue | effect       |
##Слабый голод 
#| сытость | 0         | fake-food  | сытость       | 1         | 50            | нет          |
#| сытость | 0         | fake-food  | -сытость      | 1         | -50           | Голод        |
##Голод
#| сытость | -50       | fake-food  | сытость       | 1         | 0             | нет          |
#| сытость | -50       | fake-food  | -сытость      | 1         | 0             | Голодание    |
##Голодание
#| сытость | -100      | fake-food  | сытость       | 1         | -50           | Слабый голод |
##Слабая жажда
#| вода    | 0         | fake-water | вода          | 1         | 50            | нет          |
##Жажда
#| вода    | -50       | fake-water | вода          | 1         | 0             | нет          |
##Обезвоживание
#| вода    | -100      | fake-water | вода          | 1         | -50           | Слабая жажда |

#@survival @dev1
#Scenario Outline: Употребление медикаментов для восстановления Hp.
#	Given Есть карта размером 2
#	And Есть актёр игрока класса captain в ячейке (0, 0)
#	And Актёр игрока имеет Hp: <startHp>
#	And В инвентаре у актёра есть еда: <propSid> количество: <propCount>
#	When Актёр использует предмет <propSid> на себя
#	Then Актёр игрока имеет запас hp <expectedHpValue>
#	And Предмет <propSid> отсутствует в инвентаре актёра
#
#Examples: 
#	| startHp | propSid | propCount | expectedHpValue |
#	| 1       | med-kit | 1         | 5               |

@survival @dev2
Scenario Outline: Употребление медикаментов снижает сытость и воду.
	Given Есть карта размером 2
	And Есть актёр игрока класса human-person в ячейке (0, 0)
	And Актёр игрока имеет Hp: 1
	And В инвентаре у актёра есть еда: <prop> количество: 100
	When Актёр использует предмет <prop> на себя <iterations> раз
	Then Актёр под эффектом <effect>

	Examples: 
	| iterations | effect                 | prop    |
	| 1          | Слабая токсикация      | med-kit |
	| 2          | Сильная токсикация     | med-kit |
	| 3          | Смертельная токсикация | med-kit |

@survival @dev1
Scenario Outline: Наступление выживальных состояний (жажда/голод/утомление)
	Given Есть карта размером 2
	And Есть актёр игрока класса human-person в ячейке (0, 0)
	# special perk to exclude other hazard influence
	And Актёр игрока получает перк <testPerk>
	When Я жду <iterations> итераций
	Then Актёр под эффектом <effect>

	Examples: 
	| iterations | stat    | effect        | testPerk        |
	| 250        | сытость | Слабый голод  | thrist-immunity |
	| 1000       | сытость | Голод         | thrist-immunity |
	| 3300       | сытость | Голодание     | thrist-immunity |
	| 268        | вода    | Слабая жажда  | hunger-immunity |
	| 400        | вода    | Жажда         | hunger-immunity |
	| 1288       | вода    | Обезвоживание | hunger-immunity |

@survival @dev1
Scenario Outline: Эффекты угроз выживания наносят урон актёру.
	Given Есть карта размером 2
	And Есть актёр игрока класса human-person в ячейке (0, 0)
	And Актёр имеет эффект <startEffect>
	When Я жду 1 итераций
	Then Актёр игрока имеет запас hp <expectedHpValue>

	Examples: 
	| startEffect   | moveDistance | expectedHpValue |
	| Голодание     | 1            | 119             |
	| Обезвоживание | 1            | 119             |
	| Слабый голод  | 1            | 120             |
	| Голод         | 1            | 120             |
	| Слабая жажда  | 1            | 120             |
	| Жажда         | 1            | 120             |

@survival @dev1
Scenario Outline: Угрозы выживания (имеются изначально) снижают эффективность тактических действий у актёра игрока.
	Given Есть карта размером <mapSize>
	And Есть актёр игрока класса <personSid> в ячейке (<actorNodeX>, <actorNodeY>)
	And В инвентаре у актёра игрока есть предмет: <equipmentSid>
	And Актёр имеет эффект <startEffect>
	When Экипирую предмет <equipmentSid> в слот Index: <slotIndex>
	And Жду 1000 единиц времени
	Then Тактическое умение <tacticalActSid> имеет дебафф на эффективность

Examples: 
| mapSize | personSid | actorNodeX | actorNodeY | startEffect   | equipmentSid | slotIndex | tacticalActSid |
| 2       | captain   | 0          | 0          | Слабый голод  | short-sword  | 2         | slash          |
| 2       | captain   | 0          | 0          | Голод         | short-sword  | 2         | slash          |
| 2       | captain   | 0          | 0          | Голодание     | short-sword  | 2         | slash          |
| 2       | captain   | 0          | 0          | Слабая жажда  | short-sword  | 2         | slash          |
| 2       | captain   | 0          | 0          | Жажда         | short-sword  | 2         | slash          |
| 2       | captain   | 0          | 0          | Обезвоживание | short-sword  | 2         | slash          |