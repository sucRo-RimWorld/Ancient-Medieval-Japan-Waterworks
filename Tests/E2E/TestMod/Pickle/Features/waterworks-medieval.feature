Feature: Optional real DBH for Medieval irrigation

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Pawn opens native sluice and game ticks irrigate soil
    Then a pawn opens the native Medieval sluice and its ticks irrigate soil

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Natural source feeds native sluice once and disconnects safely
    Then Medieval sluice irrigates from natural Waterworks supply and tracks disconnection

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Diagonal contact and disconnected Medieval chains never irrigate
    Then Medieval sluice rejects diagonal only and severed foreign canal chains

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Native storage is charged first and survives natural source loss
    Then native DBH tower supply takes precedence and remains after source loss

  @quickstart:WaterworksQuickstart @timeout:100
  Scenario: Real save reload preserves optional irrigation topology
    Given Medieval irrigation and Waterworks topology exist before a disk save
    When I save and reload
    Then reloaded Medieval sluice irrigates and source loss still stops it
