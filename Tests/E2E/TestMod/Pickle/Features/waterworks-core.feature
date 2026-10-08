Feature: Waterworks independent canal core

  Scenario: Waterworks production Defs load correctly
    Then Waterworks loaded Defs preserve the canal contract

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Four-direction canal branches connect and disconnect
    Then cardinal canal branches connect disconnect and reconnect

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Standing freshwater requires nine adjacent cells
    Then standing ponds use the nine cell freshwater threshold

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Vanilla bridge preserves canal flow and terrain restoration
    Then Vanilla bridge preserves water and gravel restoration

  @quickstart:WaterworksQuickstart @timeout:180
  Scenario: Construction pawn completes real dig and fill jobs
    Then a construction pawn actually digs and fills a canal
