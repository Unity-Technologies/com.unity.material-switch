# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.6.0-preview] - 2021-10-13

### Added
* feat: added a global property map to material switch clips.
* feat: use the standard color picker when no palette texture is assigned. 

### Changed
* deps: update dependency to com.unity.selection-groups@0.5.2-preview

### Fixed
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

