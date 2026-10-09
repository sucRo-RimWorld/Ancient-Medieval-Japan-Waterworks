Feature: Waterworks independent canal core

  Scenario: Waterworks production Defs load correctly
    Then Waterworks loaded Defs preserve the canal contract

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Four-direction canal branches connect and disconnect
    Then cardinal canal branches connect disconnect and reconnect

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Canal width stays one cell
    Then one cell width rejects broad canals but keeps junctions

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Standing freshwater requires nine adjacent cells
    Then standing ponds use the nine cell freshwater threshold

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Mud and Marsh allow excavation without becoming freshwater sources
    Then Mud and Marsh allow excavation but are not natural freshwater sources

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Vanilla bridge preserves canal flow and terrain restoration
    Then Vanilla bridge preserves water and gravel restoration

  @quickstart:WaterworksQuickstart @timeout:180
  Scenario: Construction pawn completes real dig and fill jobs
    Then a construction pawn actually digs and fills a canal

  @quickstart:WaterworksQuickstart @timeout:300
  Scenario: Save and reload restores canal supply and original ground
    Given Waterworks has supplied and dry canals with distinct original ground
    When I save and reload
    Then Waterworks rebuilds supply and restores both saved ground types
