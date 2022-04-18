# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.7.3-preview] - 2022-04-18

### Changed
* deps: use com.unity.film-internal-utilities@0.14.2-preview 
* deps: use com.unity.selection-groups@0.7.4-preview

## [0.7.2-preview] - 2022-04-12

### Added
* api: open API to copy and paste material properties of MaterialSwitchClip
* doc: add TableOfContents structure and installation page
* doc: add documentations on MaterialSwitchClip

### Changed
* deps: use com.unity.selection-groups@0.7.3-preview
* refactor: rename MaterialSwitchUtility (obsolete) to MaterialSwitchEditorUtility 

### Fixed
* fix: null error of activeMaterials in MaterialSwitchClipEditor 

## [0.7.1-preview] - 2022-04-05

### Changed
* deps: use com.unity.film-internal-utilities@0.14.1-preview

## [0.7.0-preview] - 2022-03-25

### Added

* feat: allow copy paste between global and per material settings (#79)
* api: open MaterialSwitchTrack, MaterialSwitchClip, MaterialProperties, and MaterialSwitchProperty to public

## [0.6.7-preview] - 2022-02-10

### Changed
* deps: update dependencies to com.unity.film-internal-utilities@0.13.0-preview
* deps: update dependencies to com.unity.selection-groups@0.7.1-preview

### Fixed
* fix: add UI to allow removal of property overrides

## [0.6.6-preview] - 2022-01-14

* re-releasing due to package distribution issues

## [0.6.5-preview] - 2022-01-12

### Changed
* deps: update dependency to com.unity.selection-groups:0.6.2-preview

## [0.6.4-preview] - 2021-12-06

### Fixed
* fix: corrected float interpolation 

## [0.6.3-preview] - 2021-11-19

### Fixed
* fix: null check for unassigned texture properties

## [0.6.2-preview] - 2021-11-17

### Changed
* deps: use com.unity.timeline@1.5.7

### Fixed
* fix: color blending between MaterialSwitchClips
* fix: update materials in MaterialSwitchClips if the materials in the bound SelectionGroup are replaced

## [0.6.1-preview] - 2021-11-12

### Changed
* deps: use com.unity.film-internal-utilities@0.12.2-preview

### Fixed
* fix: errors when adding a new MaterialSwitchClip on a new MaterialSwitchTrack
* allow handling of selection groups with different shaders
* handle new selection group members when creating a clip.
* use the current material setup of the bound selection group when creating a clip
* fix clip blending


## [0.6.0-preview] - 2021-10-13

### Added
* feat: added a global property map to material switch clips.
* feat: use the standard color picker when no palette texture is assigned. 

### Changed
* deps: update dependency to com.unity.selection-groups@0.5.2-preview

### Fixed
* don't try to sample textures that are not readable.

## [0.5.1-preview] - 2021-10-25

### Changed
* deps: update dependency to com.unity.selection-groups@0.5.4-preview
* deps: update dependency to com.unity.film-internal-utilities@0.11.1-preview

### Fixed
* fixed errors when adding new MaterialSwitchClips
* don't try to sample textures that are not readable.

## [0.5.0-preview] - 2021-09-15

### Added
* feat: multi clip editing and copy paste.

### Changed
* deps: update dependencies to com.unity.film-internal-utilities@0.11.0-preview
* deps: update dependencies to com.unity.selection-groups@0.5.0-preview

## [0.4.0-preview] - 2021-08-18

### Added
* add global palette texture field to be used as the default palette texture
* doc: put links to the official docs in the github top readme page

### Changed
* deps: update dependency to com.unity.film-internal-utilities@0.10.2-preview
* test: add test against Unity 2021.2

### Removed
* doc: delete obsolete contents in the top readme

## [0.3.1-preview] - 2021-07-14

### Changed
* convert changelog format to semantics versioning

### Fixed
* remove non-functioning "Refresh Materials" button

## [0.3.0-preview] - 2021-07-07

### Changed
* deps: update dependencies to FilmInternalUtilities and SelectionGroups 
** com.unity.film-internal-utilities: 0.10.1-preview
** com.unity.selection-groups: 0.4.1-preview

### Fixed
* apply color change on palette swap

## [0.2.1-preview] - 2021-06-09

### Fixed
* refresh when adding new materials 

## [0.2.0-preview] - 2021-03-11

### Changed
* deps: depend on com.unity.film-internal-utilities@0.8.2-preview and rename assemblies
* deps: depend on com.unity.selection-groups@0.3.1-preview 
* chore: make functions and classes to internal

### Fixed
* check null inspectedDirector during onClipChanged 

## [0.1.0-preview] - 2021-02-16

### Changed
* deps: use com.unity.selection-groups@0.2.3-preview

## [0.0.4-preview] - 2020-12-01

### Fixed
* Fixed delayed color update on material-switch clips.

## [0.0.3-preview] - 2020-10-08

### Added
* First pre-release version.

