Feature: Prepare a genuine pre-Waterworks RimWorld saved game

  @quickstart:WaterworksQuickstart @timeout:300
  Scenario: Vanilla saved game is created without Waterworks
    Then the Vanilla save baseline is prepared without Waterworks
    When I save and reload as "waterworks-before-install"
