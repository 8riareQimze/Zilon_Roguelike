﻿Feature: Survival_HarardsDamagesActor
	Чтобы была необходимость своевременно пополнять характеристики выживания
	Как игроку
	Мне нужно, чтобы актёр, находящийся под максимальной угрозой выживания терял здоровье.

@survival @dev0
Scenario Outline: Эффекты угроз выживания наносят урон актёру.
	Given Есть карта размером 2
	And Есть актёр игрока класса captain в ячейке (0, 0)
	And Актёр имеет эффект <startEffect>
	When Я перемещаю персонажа на <moveDistance> клетку
	Then Актёр игрока имеет запас hp <expectedHpValue>

	Examples: 
	| startEffect   | moveDistance | expectedHpValue |
	| Голодание     | 1            | 119             |
	| Обезвоживание | 1            | 119             |
	| Слабый голод  | 1            | 120             |
	| Голод         | 1            | 120             |
	| Слабая жажда  | 1            | 120             |
	| Жажда         | 1            | 120             |
